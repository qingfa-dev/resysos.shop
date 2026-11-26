using Testing.Unit;

namespace BuildingBlock.Persistence.EfCore.Unit.Tests;

/// <summary>Tests for <see cref="ModuleDbContext"/> as a unit of work.</summary>
public sealed class ModuleDbContextTests
{
    #region Save

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Add(order);

            var result = await context.SaveChangesAsync(0);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1);
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Should().ContainSingle()
                .Which.Name.Should().Be("first");
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDisposed_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        TestContext context = database.CreateContext();
        context.Dispose();

        var result = await context.SaveChangesAsync(0);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.disposed");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCancelled_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await context.SaveChangesAsync(0, cancellation.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.cancelled");
    }

    [Fact]
    public async Task SaveChangesAsync_WithInvalidRetryBudget_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();

        var result = await context.SaveChangesAsync(
            UnitOfWorkConstant.Constraints.MaxRetries.Max.Value + 1);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("unit_of_work.max_retries.invalid");
    }

    #endregion

    #region Transactions

    [Fact]
    public async Task BeginTransactionAsync_Commit_ShouldPersistChanges()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using (TestContext context = database.CreateContext())
        {
            var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));
            begun.IsSuccess.Should().BeTrue();

            context.Orders.Add(order);
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
            (await begun.Value.CommitAsync()).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Should().ContainSingle().Which.Id.Should().Be(order.Id);
        }
    }

    [Fact]
    public async Task BeginTransactionAsync_Rollback_ShouldDiscardChanges()
    {
        using var database = new SqliteTestDatabase();
        var order = Orders.New("first", 10);

        await using (TestContext context = database.CreateContext())
        {
            var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));
            context.Orders.Add(order);
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
            (await begun.Value.RollbackAsync()).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = database.CreateContext())
        {
            context.Orders.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task BeginTransactionAsync_WithInvalidTimeout_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();

        var result = await context.BeginTransactionAsync(TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.timeout.invalid");
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenAlreadyActive_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));

        var result = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.active");

        await begun.Value.RollbackAsync();
    }

    [Fact]
    public async Task CommitAsync_WhenAlreadyCompleted_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));
        (await begun.Value.CommitAsync()).IsSuccess.Should().BeTrue();

        var result = await begun.Value.CommitAsync();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.completed");
    }

    [Fact]
    public async Task CommitAsync_WhenExpired_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        var clock = TestClock.Create();
        await using TestContext context = database.CreateContext(clock);
        var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(1));

        clock.Advance(TimeSpan.FromSeconds(2));
        var result = await begun.Value.CommitAsync();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.expired");
    }

    [Fact]
    public async Task Transaction_WhenDisposed_ShouldReturnError()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();
        var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));

        await begun.Value.DisposeAsync();
        var result = await begun.Value.CommitAsync();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.disposed");
    }

    #endregion

    #region Model

    [Fact]
    public async Task Model_ShouldMarkVersionAsConcurrencyToken()
    {
        using var database = new SqliteTestDatabase();
        await using TestContext context = database.CreateContext();

        var property = context.Model
            .FindEntityType(typeof(VersionedOrder))!
            .FindProperty(nameof(IVersionable.Version));

        property.Should().NotBeNull();
        property!.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_SoftDeletable_ShouldMarkDeletedAndFilter()
    {
        using var database = new SqliteTestDatabase();
        var clock = TestClock.Create();
        await using TestContext context = database.CreateContext(clock);
        var order = Orders.Soft("doomed");
        context.SoftOrders.Add(order);
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();

        context.SoftOrders.Remove(order);
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();

        order.IsDeleted.Should().BeTrue();
        order.DeletedBy.Should().Be("tester");
        order.DeletedAtUtc.Should().Be(clock.GetUtcNow());
        context.SoftOrders.Should().BeEmpty();
        context.SoftOrders.IgnoreQueryFilters().Should().ContainSingle();
    }

    #endregion
}
