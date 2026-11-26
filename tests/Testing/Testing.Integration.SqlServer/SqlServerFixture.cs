using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using Respawn;

using Testing.Integration.Containers;

namespace Testing.Integration.SqlServer;

/// <summary>
/// Owns a throwaway SQL Server instance for integration tests. Start it once per collection and
/// reset state between tests with <see cref="ResetAsync"/>.
/// </summary>
/// <remarks>
/// The container is built directly instead of through <c>MsSqlBuilder</c>: that builder probes
/// readiness and creates the database with <c>sqlcmd</c> exec sessions, and rootless Podman denies
/// the <c>oom_score_adj</c> write that crun performs for every exec. This fixture waits on the
/// server log, confirms the connection from the host, and creates the database over TDS.
/// </remarks>
public sealed class SqlServerFixture : IAsyncDisposable
{
    static SqlServerFixture() => ContainerRuntime.Configure();

    private const ushort SqlServerPort = 1433;

    private readonly IContainer _container;
    private readonly SqlServerTestOptions _options;
    private Respawner? _respawner;

    /// <summary>Initializes the fixture.</summary>
    /// <param name="options">Container settings, or <see langword="null"/> for the defaults.</param>
    public SqlServerFixture(SqlServerTestOptions? options = null)
    {
        _options = options ?? new SqlServerTestOptions();

        _container = new ContainerBuilder(_options.Image)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", _options.Password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithPortBinding(SqlServerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("SQL Server is now ready for client connections"))
            .Build();
    }

    /// <summary>Gets the connection string of the configured database.</summary>
    public string ConnectionString => BuildConnectionString(_options.Database);

    /// <summary>Gets the connection string of the <c>master</c> database.</summary>
    public string MasterConnectionString => BuildConnectionString("master");

    /// <summary>Starts the container, creates the database and waits until it accepts connections.</summary>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _container.StartAsync(cancellationToken).ConfigureAwait(false);
        await WaitForConnectionAsync(MasterConnectionString, cancellationToken).ConfigureAwait(false);
        await CreateDatabaseAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the rows of every table in the supplied schemas with Respawn, keeping the schemas and
    /// their tables. When no schema is supplied, the <c>dbo</c> schema is reset.
    /// </summary>
    /// <param name="schemas">The schemas to reset.</param>
    public async Task ResetAsync(params string[] schemas)
    {
        string[] include = schemas.Length == 0 ? ["dbo"] : schemas;

        await using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        if (!await HasTablesAsync(connection, include).ConfigureAwait(false))
        {
            return;
        }

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = include,
        }).ConfigureAwait(false);

        await _respawner.ResetAsync(connection).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _container.DisposeAsync().ConfigureAwait(false);

    private string BuildConnectionString(string database)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{_container.Hostname},{_container.GetMappedPublicPort(SqlServerPort)}",
            InitialCatalog = database,
            UserID = "sa",
            Password = _options.Password,
            Encrypt = false,
            TrustServerCertificate = true,
            ConnectTimeout = 60,
        };

        return builder.ConnectionString;
    }

    private static async Task WaitForConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(120);

        while (true)
        {
            try
            {
                await using SqlConnection connection = new(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (SqlException) when (DateTimeOffset.UtcNow < deadline)
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new(MasterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{_options.Database}') IS NULL
            BEGIN
                CREATE DATABASE [{_options.Database}]
            END
            """;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<bool> HasTablesAsync(
        SqlConnection connection,
        string[] schemas)
    {
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name IN (SELECT value FROM STRING_SPLIT(@schemas, ','))
            """;
        command.Parameters.AddWithValue("schemas", string.Join(",", schemas));

        int count = (int)(await command.ExecuteScalarAsync().ConfigureAwait(false) ?? 0);
        return count > 0;
    }
}

/// <summary>Registration for the SQL Server integration fixture.</summary>
public static class SqlServerFixtureExtensions
{
    /// <summary>Registers the fixture and its container settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings overrides.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSqlServerTestFixture(
        this IServiceCollection services,
        Action<SqlServerTestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new SqlServerTestOptions();
        configure?.Invoke(options);
        IReadOnlyList<string> errors = options.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The SQL Server fixture settings are invalid: " + string.Join(" ", errors));
        }

        services.AddSingleton(options);
        services.AddSingleton(provider => new SqlServerFixture(
            provider.GetRequiredService<SqlServerTestOptions>()));
        return services;
    }
}
