using BuildingBlock.Persistence.EfCore.Interceptors;

using Testing.Integration.PostgreSql;

using SharedTestData = Testing.Data.TestData;

namespace BuildingBlock.Persistence.PostgreSql.Integration.Tests;

#region Aggregates

public sealed class Order : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Amount { get; set; }
}

public sealed class SoftOrder : IIdentifiable<Guid>, ISoftDeletable, ICreatable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public string? DeletedBy { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }
}

public sealed class VersionedOrder : IIdentifiable<Guid>, IVersionable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long Version { get; set; }
}

#endregion

#region Context

public sealed class TestContext : ModuleDbContext
{
    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<SoftOrder> SoftOrders => Set<SoftOrder>();

    public DbSet<VersionedOrder> VersionedOrders => Set<VersionedOrder>();

    protected override string SchemaName => PostgreSqlDatabaseFixture.Schema;

    protected override string ModuleName => "Test";
}

#endregion

#region Data

internal static class Orders
{
    public static Order New(string name, int amount)
        => new() { Id = SharedTestData.Id(), Name = name, Amount = amount };

    public static SoftOrder Soft(string name, string createdBy = "tester")
        => new()
        {
            Id = SharedTestData.Id(),
            Name = name,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

    public static VersionedOrder Versioned(string name)
        => new() { Id = SharedTestData.Id(), Name = name, Version = 1 };
}

#endregion

#region Fixture

/// <summary>
/// Starts one PostgreSQL container for the integration collection, creates the module schema,
/// and resets it with Respawn between tests.
/// </summary>
public sealed class PostgreSqlDatabaseFixture : IAsyncLifetime
{
    /// <summary>The module schema exercised by the integration tests.</summary>
    public const string Schema = "test";

    private readonly PostgresFixture _fixture = new();

    /// <summary>Gets a value indicating whether a container runtime was found and started.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Gets the connection string of the running container.</summary>
    public string ConnectionString => _fixture.ConnectionString;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        if (!Testing.Integration.Containers.ContainerRuntime.IsAvailable())
        {
            return;
        }

        try
        {
            await _fixture.StartAsync();
            IsAvailable = true;

            await using TestContext context = CreateContext();
            await context.Database.EnsureCreatedAsync();
        }
        catch
        {
            await _fixture.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (IsAvailable)
        {
            await _fixture.DisposeAsync();
        }
    }

    /// <summary>Creates a context bound to the running container.</summary>
    /// <returns>The module context.</returns>
    public TestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;

        return new TestContext(options);
    }

    /// <summary>Truncates the module schema with Respawn.</summary>
    /// <returns>A task that completes when the schema is empty.</returns>
    public Task ResetAsync() => _fixture.ResetAsync(Schema);
}

/// <summary>The shared PostgreSQL collection.</summary>
[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlDatabaseFixture>
{
    /// <summary>The collection name.</summary>
    public const string Name = "postgresql";
}

/// <summary>Skips a test at discovery when no Docker or Podman endpoint is available.</summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    /// <summary>Initializes the attribute.</summary>
    /// <param name="sourceFilePath">Provided by the compiler.</param>
    /// <param name="sourceLineNumber">Provided by the compiler.</param>
    public PostgresFactAttribute(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!Testing.Integration.Containers.ContainerRuntime.IsAvailable())
        {
            Skip = "No Docker or Podman API endpoint is available. Start Docker, or install "
                + "Podman so the test process can start its API service.";
        }
    }
}

#endregion
