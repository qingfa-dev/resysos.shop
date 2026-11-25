namespace BuildingBlock.Querying.EfCore.Unit.Tests;

#region Entities

/// <summary>A product entity used by the EF Core executor tests.</summary>
internal sealed class Product
{
    /// <summary>Gets or sets the product identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the product price.</summary>
    public decimal Price { get; set; }
}

#endregion

#region Context

/// <summary>The test <see cref="DbContext"/> exposing the product set.</summary>
internal sealed class TestDbContext : DbContext
{
    /// <summary>Initializes a new instance of the <see cref="TestDbContext"/> class.</summary>
    /// <param name="options">The context options.</param>
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the product set.</summary>
    public DbSet<Product> Products => Set<Product>();
}

#endregion

#region Database

/// <summary>An in-memory SQLite database used by the executor tests.</summary>
internal sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestDbContext _context;

    private TestDb(SqliteConnection connection, TestDbContext context)
    {
        _connection = connection;
        _context = context;
    }

    /// <summary>Gets the product set of the test database.</summary>
    public DbSet<Product> Products => _context.Products;

    /// <summary>Persists pending changes to the test database.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of state entries written.</returns>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    /// <summary>Creates a fresh in-memory SQLite database, optionally seeded with products.</summary>
    /// <param name="products">The number of products to seed.</param>
    /// <returns>The test database.</returns>
    public static TestDb Create(int products = 0)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(options);
        context.Database.EnsureCreated();

        for (int index = 0; index < products; index++)
        {
            context.Products.Add(new Product
            {
                Id = index + 1,
                Name = $"Product {index + 1}",
                Price = 100m + index
            });
        }

        context.SaveChanges();

        return new TestDb(connection, context);
    }

    /// <summary>Disposes the context and its underlying connection.</summary>
    /// <returns>A task that completes when disposal finishes.</returns>
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

#endregion
