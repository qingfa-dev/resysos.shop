using BuildingBlock.Persistence.EfCore.Interceptors;

using Microsoft.Data.Sqlite;

using SharedTestData = Testing.Data.TestData;

namespace BuildingBlock.Persistence.EfCore.Unit.Tests;

#region Aggregates

internal sealed class Order : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Amount { get; set; }
}

internal sealed class SoftOrder : IIdentifiable<Guid>, ISoftDeletable, ICreatable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public string? DeletedBy { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }
}

internal sealed class VersionedOrder : IIdentifiable<Guid>, IVersionable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long Version { get; set; }
}

#endregion

#region Context

internal sealed class TestContext : ModuleDbContext
{
    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public TimeProvider? Clock { get; set; }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<SoftOrder> SoftOrders => Set<SoftOrder>();

    public DbSet<VersionedOrder> VersionedOrders => Set<VersionedOrder>();

    protected override string SchemaName => "test";

    protected override string ModuleName => "Test";

    protected override TimeProvider TimeSource => Clock ?? TimeProvider.System;
}

#endregion

#region Database

internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public TestContext CreateContext(TimeProvider? clock = null)
    {
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;

        var context = new TestContext(options) { Clock = clock };
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose() => _connection.Dispose();
}

#endregion

#region Data

internal static class Orders
{
    public static Order New(string name, int amount)
        => new() { Id = SharedTestData.Id(), Name = name, Amount = amount };

    public static Order WithId(Guid id, string name, int amount)
        => new() { Id = id, Name = name, Amount = amount };

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
