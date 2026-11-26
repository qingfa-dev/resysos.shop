using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;

namespace Testing.Integration.Containers;

/// <summary>
/// Points Testcontainers at whichever container engine is available.
/// </summary>
/// <remarks>
/// Testcontainers' endpoint discovery only probes Docker sockets. Podman is API-compatible, but its
/// socket is published only while <c>podman system service</c> runs, so a machine with Podman
/// installed still looks unavailable at discovery time. This helper closes that gap:
/// <list type="bullet">
/// <item>an explicit <c>DOCKER_HOST</c>, <c>DOCKER_CONTEXT</c>, or a reachable Docker socket wins;</item>
/// <item>an already running Podman API socket is reused;</item>
/// <item>otherwise, when the <c>podman</c> CLI is installed, a private API service is started for the
/// current test process and removed when the process exits.</item>
/// </list>
/// Rootless Podman cannot bind-mount its socket into Ryuk, so the resource reaper is disabled for
/// Podman endpoints. Explicit <c>DOCKER_HOST</c>, <c>DOCKER_CONTEXT</c>, and
/// <c>TESTCONTAINERS_RYUK_DISABLED</c> values always win over the detection below.
/// </remarks>
public static class ContainerRuntime
{
    private const string DockerHostVariable = "DOCKER_HOST";
    private const string DockerContextVariable = "DOCKER_CONTEXT";
    private const string RyukDisabledVariable = "TESTCONTAINERS_RYUK_DISABLED";
    private const string XdgRuntimeDirVariable = "XDG_RUNTIME_DIR";
    private const string TempVariable = "TMPDIR";
    private const string PathVariable = "PATH";
    private const string PodmanExecutable = "podman";

    private static readonly string[] PodmanMarkers = ["podman", "libpod"];

    private static readonly object Gate = new();

    private static bool _resolved;
    private static string? _dockerHost;
    private static bool _disableRyuk;
    private static bool _available;
    private static Process? _ownedService;
    private static string? _ownedServiceSocket;

    /// <summary>
    /// Applies the detected runtime settings to the current process. Call this before building a
    /// container; the fixtures in this library already do.
    /// </summary>
    public static void Configure()
    {
        RuntimeDecision decision = Resolve();

        if (decision.DockerHost is { } dockerHost)
        {
            Environment.SetEnvironmentVariable(DockerHostVariable, dockerHost);
        }

        if (decision.DisableRyuk)
        {
            Environment.SetEnvironmentVariable(RyukDisabledVariable, "true");
        }
    }

    /// <summary>
    /// Reports whether a Docker or Podman endpoint can be used. When Podman is installed but its API
    /// service is not running, this starts a private service so the tests run instead of skipping.
    /// </summary>
    public static bool IsAvailable() => Resolve().IsAvailable;

    /// <summary>
    /// Works out which settings to apply without touching ambient state. Every input is injected so
    /// the Docker, Podman, and explicit-configuration branches can be exercised by unit tests.
    /// </summary>
    /// <param name="isWindows">Whether the host runs Windows.</param>
    /// <param name="homeDirectory">The current user's home directory.</param>
    /// <param name="getEnvironmentVariable">Reads an environment variable.</param>
    /// <param name="socketExists">Reports whether a socket path exists.</param>
    public static ContainerRuntimeDecision Decide(
        bool isWindows,
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable,
        Func<string, bool> socketExists)
    {
        ArgumentNullException.ThrowIfNull(getEnvironmentVariable);
        ArgumentNullException.ThrowIfNull(socketExists);

        string? dockerHost = getEnvironmentVariable(DockerHostVariable);

        // An explicit endpoint always wins. Ryuk still has to be turned off when that endpoint is
        // Podman, because rootless Podman cannot bind-mount its socket into the reaper.
        if (!string.IsNullOrEmpty(dockerHost))
        {
            return new ContainerRuntimeDecision(
                DisableRyuk: LooksLikePodman(dockerHost) && !IsRyukSettingExplicit(getEnvironmentVariable));
        }

        // Docker Desktop handles its own discovery (including named pipes on Windows), and an
        // explicit context may point anywhere, so leave both alone.
        if (isWindows || !string.IsNullOrEmpty(getEnvironmentVariable(DockerContextVariable)))
        {
            return default;
        }

        if (HasDockerSocket(homeDirectory, getEnvironmentVariable, socketExists))
        {
            return default;
        }

        string? podmanSocket = FindPodmanSocket(homeDirectory, getEnvironmentVariable, socketExists);

        return podmanSocket is null
            ? default
            : new ContainerRuntimeDecision(
                $"unix://{podmanSocket}",
                DisableRyuk: !IsRyukSettingExplicit(getEnvironmentVariable));
    }

    private static RuntimeDecision Resolve()
    {
        lock (Gate)
        {
            if (!_resolved)
            {
                RuntimeDecision decision = ResolveCore();

                _dockerHost = decision.DockerHost;
                _disableRyuk = decision.DisableRyuk;
                _available = decision.IsAvailable;
                _resolved = true;
            }

            return new RuntimeDecision(_dockerHost, _disableRyuk, _available);
        }
    }

    private static RuntimeDecision ResolveCore()
    {
        string? dockerHost = Environment.GetEnvironmentVariable(DockerHostVariable);

        if (!string.IsNullOrEmpty(dockerHost))
        {
            return new RuntimeDecision(
                dockerHost,
                LooksLikePodman(dockerHost) && !IsRyukSettingExplicit(Environment.GetEnvironmentVariable),
                IsAvailable: true);
        }

        if (OperatingSystem.IsWindows()
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DockerContextVariable)))
        {
            return new RuntimeDecision(null, false, IsAvailable: true);
        }

        string? homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (HasDockerSocket(homeDirectory, Environment.GetEnvironmentVariable, CanConnect))
        {
            return new RuntimeDecision(null, false, IsAvailable: true);
        }

        string? podmanSocket = FindPodmanSocket(
            homeDirectory,
            Environment.GetEnvironmentVariable,
            CanConnect);

        if (podmanSocket is not null)
        {
            return new RuntimeDecision(
                $"unix://{podmanSocket}",
                !IsRyukSettingExplicit(Environment.GetEnvironmentVariable),
                IsAvailable: true);
        }

        string? startedSocket = StartPodmanService();

        return startedSocket is null
            ? new RuntimeDecision(null, false, IsAvailable: false)
            : new RuntimeDecision(
                $"unix://{startedSocket}",
                !IsRyukSettingExplicit(Environment.GetEnvironmentVariable),
                IsAvailable: true);
    }

    private static string? StartPodmanService()
    {
        string? podman = FindExecutable(PodmanExecutable);

        if (podman is null)
        {
            return null;
        }

        string? socketPath = PodmanServiceSocketPath();

        if (socketPath is null)
        {
            return null;
        }

        if (CanConnect(socketPath))
        {
            return socketPath;
        }

        // A killed service can leave its socket behind, and a recycled process id would then make
        // the new service fail to bind. Remove the stale entry before starting.
        try
        {
            File.Delete(socketPath);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = podman,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("system");
            startInfo.ArgumentList.Add("service");
            startInfo.ArgumentList.Add("--time=0");
            startInfo.ArgumentList.Add($"unix://{socketPath}");

            _ownedService = Process.Start(startInfo);
            _ownedServiceSocket = socketPath;
        }
        catch (Win32Exception)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        if (_ownedService is null)
        {
            return null;
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopOwnedService();

        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(15);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (CanConnect(socketPath))
            {
                return socketPath;
            }

            Thread.Sleep(100);
        }

        StopOwnedService();
        return null;
    }

    private static string? PodmanServiceSocketPath()
    {
        string root = Environment.GetEnvironmentVariable(XdgRuntimeDirVariable)
            ?? Environment.GetEnvironmentVariable(TempVariable)
            ?? Path.GetTempPath();
        string directory = Path.Combine(root, "podman");

        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        // A per-process socket keeps concurrently running test projects from sharing (and then
        // killing) one service when each of them owns the daemon it started.
        return Path.Combine(directory, $"podman-{Environment.ProcessId}.sock");
    }

    private static bool CanConnect(string socketPath)
    {
        if (OperatingSystem.IsWindows())
        {
            return File.Exists(socketPath);
        }

        try
        {
            using var socket = new Socket(
                AddressFamily.Unix,
                SocketType.Stream,
                ProtocolType.Unspecified);
            socket.Connect(new UnixDomainSocketEndPoint(socketPath));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }

    private static string? FindExecutable(string name)
    {
        string? path = Environment.GetEnvironmentVariable(PathVariable);

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string[] extensions = OperatingSystem.IsWindows() ? [".exe", ".cmd", ".bat"] : [string.Empty];

        foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (string extension in extensions)
            {
                string candidate = Path.Combine(directory, name + extension);

                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static void StopOwnedService()
    {
        Process? process = _ownedService;
        string? socketPath = _ownedServiceSocket;
        _ownedService = null;
        _ownedServiceSocket = null;

        if (process is not null)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
                // The service already exited.
            }
            catch (Win32Exception)
            {
                // The process could not be killed. It serves no containers after the tests end, so
                // it is left for the operator or the machine lifecycle to clean up.
            }
            finally
            {
                process.Dispose();
            }
        }

        if (socketPath is null)
        {
            return;
        }

        // Kill skips the daemon's own shutdown, so the socket file has to be removed here.
        try
        {
            File.Delete(socketPath);
        }
        catch (IOException)
        {
            // The file is already gone or still mapped; the next run uses a new process id anyway.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above: a per-process name keeps the stale entry harmless.
        }
    }

    private static bool HasDockerSocket(
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable,
        Func<string, bool> socketExists)
    {
        foreach (string? candidate in DockerSocketCandidates(homeDirectory, getEnvironmentVariable))
        {
            if (candidate is not null && socketExists(candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static string? FindPodmanSocket(
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable,
        Func<string, bool> socketExists)
    {
        foreach (string? candidate in PodmanSocketCandidates(homeDirectory, getEnvironmentVariable))
        {
            if (candidate is not null && socketExists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string?> DockerSocketCandidates(
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable)
    {
        yield return "/var/run/docker.sock";
        yield return Combine(getEnvironmentVariable(XdgRuntimeDirVariable), "docker.sock");
        yield return Combine(homeDirectory, ".docker", "run", "docker.sock");
    }

    private static IEnumerable<string?> PodmanSocketCandidates(
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable)
    {
        // Linux, rootless.
        yield return Combine(getEnvironmentVariable(XdgRuntimeDirVariable), "podman", "podman.sock");
        // Linux, rootful.
        yield return "/run/podman/podman.sock";
        // macOS / Windows podman machine: symlink maintained by the CLI.
        yield return Combine(homeDirectory, ".local", "share", "containers", "podman", "machine", "podman.sock");
        // Older podman machine layouts.
        yield return Combine(homeDirectory, ".local", "share", "containers", "podman", "machine", "qemu", "podman.sock");
        yield return Combine(getEnvironmentVariable(TempVariable), "podman", "podman-machine-default-api.sock");
    }

    private static bool LooksLikePodman(string dockerHost) =>
        PodmanMarkers.Any(marker => dockerHost.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static bool IsRyukSettingExplicit(Func<string, string?> getEnvironmentVariable) =>
        !string.IsNullOrEmpty(getEnvironmentVariable(RyukDisabledVariable));

    private static string? Combine(string? root, params string[] parts)
    {
        if (string.IsNullOrEmpty(root))
        {
            return null;
        }

        string[] segments = new string[parts.Length + 1];
        segments[0] = root;
        parts.CopyTo(segments, 1);

        return Path.Combine(segments);
    }

    private readonly record struct RuntimeDecision(
        string? DockerHost,
        bool DisableRyuk,
        bool IsAvailable);
}

/// <summary>What <see cref="ContainerRuntime.Decide"/> concluded for the current machine.</summary>
/// <param name="DockerHost">The value to publish as <c>DOCKER_HOST</c>, or <see langword="null"/> to leave it alone.</param>
/// <param name="DisableRyuk">Whether to publish <c>TESTCONTAINERS_RYUK_DISABLED=true</c>.</param>
public readonly record struct ContainerRuntimeDecision(string? DockerHost = null, bool DisableRyuk = false);
