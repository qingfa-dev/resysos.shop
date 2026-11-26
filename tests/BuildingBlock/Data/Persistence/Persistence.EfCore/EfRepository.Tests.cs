namespace BuildingBlock.Persistence.EfCore.Unit.Tests;

/// <summary>Tests for <see cref="EfRepository{TAggregate, TId}"/>.</summary>
public sealed class EfRepositoryTests
{
    #region Add

    [Fact]
    public async Task AddAsync_ShouldPersistAggregate()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using (TestContext context = database.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);

            var result = await repository.AddAsync(order);

            result.IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Should().ContainSingle().Which.Id.Should().Be(order.Id);
        }
    }

    [Fact]
    public async Task AddAsync_NullAggregate_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.AddAsync(null!);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.aggregate.required");
    }

    [Fact]
    public async Task AddAsync_DefaultId_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.AddAsync(Orders.WithId(Guid.Empty, "x", 1));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.id.required");
    }

    [Fact]
    public async Task AddAsync_Duplicate_ShouldReturnConflictError()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using (TestContext context = database.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);
            (await repository.AddAsync(order)).IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);

            var result = await repository.AddAsync(Orders.WithId(order.Id, "duplicate", 10));

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.conflict");
        }
    }

    #endregion

    #region Find

    [Fact]
    public async Task FindAsync_ExistingAggregate_ShouldSucceed()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);
        await Seed(database, order);

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.FindAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("first");
    }

    [Fact]
    public async Task FindAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.FindAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.not_found");
    }

    [Fact]
    public async Task FindAsync_DefaultId_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.FindAsync(Guid.Empty);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.id.required");
    }

    #endregion

    #region Update and Remove

    [Fact]
    public async Task UpdateAsync_ExistingAggregate_ShouldUpdate()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);
        await Seed(database, order);

        await using (TestContext context = database.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);

            var result = await repository.UpdateAsync(Orders.WithId(order.Id, "renamed", 20));

            result.IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Single().Name.Should().Be("renamed");
        }
    }

    [Fact]
    public async Task UpdateAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.UpdateAsync(Orders.New("ghost", 1));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.not_found");
    }

    [Fact]
    public async Task RemoveAsync_ExistingAggregate_ShouldRemove()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);
        await Seed(database, order);

        await using (TestContext context = database.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);
            Order tracked = (await repository.FindAsync(order.Id)).Value;

            var result = await repository.RemoveAsync(tracked);

            result.IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task RemoveAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.RemoveAsync(Orders.New("ghost", 1));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.not_found");
    }

    #endregion

    #region Queries

    [Fact]
    public async Task ExistsAsync_ShouldReportPresence()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);
        await Seed(database, order);

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        (await repository.ExistsAsync(order.Id)).Value.Should().BeTrue();
        (await repository.ExistsAsync(Guid.NewGuid())).Value.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_ShouldFilterAggregates()
    {
        using var database = new SqliteTestDatabase();
        await Seed(
            database,
            Orders.New("small", 10),
            Orders.New("large", 100),
            Orders.New("huge", 1_000));

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.ListAsync(order => order.Amount >= 100);

        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListAsync_NullPredicate_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.ListAsync(null!);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.predicate.required");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchOrNotFound()
    {
        using var database = new SqliteTestDatabase();
        await Seed(database, Orders.New("small", 10), Orders.New("large", 100));

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        (await repository.FirstOrDefaultAsync(order => order.Amount >= 50))
            .Value.Name.Should().Be("large");

        var missing = await repository.FirstOrDefaultAsync(order => order.Amount > 1_000);

        missing.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.not_found");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_NullPredicate_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.FirstOrDefaultAsync(null!);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.predicate.required");
    }

    [Fact]
    public async Task CountAsync_ShouldCountMatches()
    {
        using var database = new SqliteTestDatabase();
        await Seed(
            database,
            Orders.New("a", 10),
            Orders.New("b", 20),
            Orders.New("c", 30));

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        (await repository.CountAsync(order => order.Amount >= 20)).Value.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_NullPredicate_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order, Guid>(context);

        var result = await repository.CountAsync(null!);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.predicate.required");
    }

    [Fact]
    public async Task GuidRepository_ShouldWorkWithoutAnExplicitId()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using TestContext context = database.CreateContext();
        var repository = new EfRepository<Order>(context);

        (await repository.AddAsync(order)).IsSuccess.Should().BeTrue();
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        (await repository.ExistsAsync(order.Id)).Value.Should().BeTrue();
    }

    #endregion

    #region Helpers

    private static async Task Seed(SqliteTestDatabase database, params Order[] orders)
    {
        await using TestContext context = database.CreateContext();
        context.Orders.AddRange(orders);
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
    }

    #endregion
}
