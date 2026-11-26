using DotNet.Testcontainers.Builders;

using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testing.Integration.Containers;

namespace Testing.Integration.PostgreSql;

/// <summary>
/// Owns a throwaway PostgreSQL instance for integration tests. Start it once per collection
/// and reset state between tests with <see cref="ResetAsync"/>.
/// </summary>
/// <remarks>
/// Works with Docker and Podman through <see cref="ContainerRuntime"/>. The fixture waits on the
/// published port from the host instead of running commands inside the container, because rootless
/// Podman denies the OOM-score write that <c>exec</c> performs on some hosts.
/// </remarks>
public sealed class PostgresFixture : IAsyncDisposable
{
    static PostgresFixture() => ContainerRuntime.Configure();

    private readonly PostgreSqlContainer _container;
    private readonly PostgreSqlTestOptions _options;
    private readonly Dictionary<string, Respawner> _respawners = new(StringComparer.Ordinal);

    /// <summary>Initializes the fixture.</summary>
    /// <param name="options">Container settings, or <see langword="null"/> for the defaults.</param>
    public PostgresFixture(PostgreSqlTestOptions? options = null)
    {
        _options = options ?? new PostgreSqlTestOptions();

        // Replace the image's pg_isready exec probe with a log-based wait. Rootless Podman
        // denies the oom_score_adj write that crun performs for exec sessions, which makes every
        // exec fail; StartAsync then waits until the server logs that it accepts connections, and
        // WaitForConnectionAsync confirms it from the host.
        _container = new PostgreSqlBuilder(_options.Image)
            .WithDatabase(_options.Database)
            .WithUsername(_options.Username)
            .WithPassword(_options.Password)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();
    }

    /// <summary>Gets the connection string of the running container.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>Starts the container and waits until it accepts connections.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _container.StartAsync(cancellationToken).ConfigureAwait(false);
        await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Truncates every table in the supplied schemas, keeping the schemas themselves. When no
    /// schema is supplied, the public schema is reset. Does nothing when the schemas have no
    /// tables yet.
    /// </summary>
    /// <param name="schemas">The schemas to reset.</param>
    public async Task ResetAsync(params string[] schemas)
    {
        string[] include = schemas.Length == 0 ? ["public"] : schemas;

        await using NpgsqlConnection connection = new(ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        if (!await HasTablesAsync(connection, include).ConfigureAwait(false))
        {
            return;
        }

        string key = string.Join(",", include);
        if (!_respawners.TryGetValue(key, out Respawner? respawner))
        {
            respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = include,
            }).ConfigureAwait(false);

            _respawners[key] = respawner;
        }

        await respawner.ResetAsync(connection).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _container.DisposeAsync().ConfigureAwait(false);

    private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(60);

        while (true)
        {
            try
            {
                await using NpgsqlConnection connection = new(ConnectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (NpgsqlException) when (DateTimeOffset.UtcNow < deadline)
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task<bool> HasTablesAsync(
        NpgsqlConnection connection,
        string[] schemas)
    {
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = ANY(@schemas)
                  AND table_type = 'BASE TABLE')
            """;
        command.Parameters.AddWithValue("schemas", schemas);

        return (bool)(await command.ExecuteScalarAsync().ConfigureAwait(false) ?? false);
    }
}

/// <summary>Registration for the PostgreSQL integration fixture.</summary>
public static class PostgresFixtureExtensions
{
    /// <summary>Registers the fixture and its container settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings overrides.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddPostgreSqlTestFixture(
        this IServiceCollection services,
        Action<PostgreSqlTestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new PostgreSqlTestOptions();
        configure?.Invoke(options);
        IReadOnlyList<string> errors = options.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The PostgreSQL fixture settings are invalid: " + string.Join(" ", errors));
        }

        services.AddSingleton(options);
        services.AddSingleton(provider => new PostgresFixture(
            provider.GetRequiredService<PostgreSqlTestOptions>()));
        return services;
    }
}
