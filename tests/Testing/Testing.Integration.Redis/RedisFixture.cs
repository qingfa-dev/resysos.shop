using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Testing.Integration.Containers;

namespace Testing.Integration.Redis;

/// <summary>Owns a throwaway Redis instance for integration tests.</summary>
/// <remarks>
/// Works with Docker and Podman through <see cref="ContainerRuntime"/>. Readiness is checked on the
/// published host port, which avoids container <c>exec</c> calls that rootless Podman can deny.
/// </remarks>
public sealed class RedisFixture : IAsyncDisposable
{
    private const int ContainerPort = 6379;

    static RedisFixture() => ContainerRuntime.Configure();

    private readonly IContainer _container;

    /// <summary>Initializes the fixture.</summary>
    /// <param name="options">Container settings, or <see langword="null"/> for the defaults.</param>
    public RedisFixture(RedisTestOptions? options = null)
    {
        string image = (options ?? new RedisTestOptions()).Image;
        _container = new ContainerBuilder(image)
            .WithPortBinding(ContainerPort, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(ContainerPort))
            .Build();
    }

    /// <summary>Gets the connection string of the running container.</summary>
    public string ConnectionString => $"{_container.Hostname}:{_container.GetMappedPublicPort(ContainerPort)}";

    /// <summary>Starts the container. Call once per test collection.</summary>
    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _container.StartAsync(cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

/// <summary>Registration for the Redis integration fixture.</summary>
public static class RedisFixtureExtensions
{
    /// <summary>Registers the fixture and its container settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings overrides.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddRedisTestFixture(
        this IServiceCollection services,
        Action<RedisTestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new RedisTestOptions();
        configure?.Invoke(options);
        IReadOnlyList<string> errors = options.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The Redis fixture settings are invalid: " + string.Join(" ", errors));
        }

        services.AddSingleton(options);
        services.AddSingleton(provider => new RedisFixture(
            provider.GetRequiredService<RedisTestOptions>()));
        return services;
    }
}
