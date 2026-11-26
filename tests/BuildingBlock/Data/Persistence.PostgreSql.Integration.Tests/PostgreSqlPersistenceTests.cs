using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Persistence.PostgreSql.Integration.Tests;

/// <summary>PostgreSQL integration tests for the EF Core persistence layer.</summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlPersistenceTests
{
    private readonly PostgreSqlDatabaseFixture _fixture;

    /// <summary>Initializes the test class with the shared container.</summary>
    /// <param name="fixture">The PostgreSQL collection fixture.</param>
    public PostgreSqlPersistenceTests(PostgreSqlDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region Connection

    [PostgresFact]
    public async Task CanConnect_ShouldSucceed()
    {
        await _fixture.ResetAsync();

        await using TestContext context = _fixture.CreateContext();

        (await context.Database.CanConnectAsync()).Should().BeTrue();
    }

    [PostgresFact]
    public async Task Model_ShouldUseTheModuleSchema()
    {
        await _fixture.ResetAsync();

        await using TestContext context = _fixture.CreateContext();

        context.Model.GetDefaultSchema().Should().Be(PostgreSqlDatabaseFixture.Schema);
    }

    #endregion

    #region Save

    [PostgresFact]
    public async Task SaveChanges_ShouldPersistAcrossContexts()
    {
        await _fixture.ResetAsync();
        var order = Orders.New("first", 10);

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Add(order);

            var result = await context.SaveChangesAsync(0);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1);
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            Order? persisted = await context.Orders.SingleOrDefaultAsync(
                candidate => candidate.Id == order.Id);

            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be("first");
            persisted.Amount.Should().Be(10);
        }
    }

    #endregion

    #region Repository

    [PostgresFact]
    public async Task Repository_ShouldRoundTripAggregates()
    {
        await _fixture.ResetAsync();
        var order = Orders.New("first", 10);

        await using (TestContext context = _fixture.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);

            (await repository.AddAsync(order)).IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);

            Order tracked = (await repository.FindAsync(order.Id)).Value;
            tracked.Name.Should().Be("first");
            (await repository.ExistsAsync(order.Id)).Value.Should().BeTrue();
            (await repository.CountAsync(candidate => candidate.Amount >= 10)).Value.Should().Be(1);

            tracked.Name = "renamed";
            tracked.Amount = 20;
            (await repository.UpdateAsync(tracked)).IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            var repository = new EfRepository<Order, Guid>(context);
            Order tracked = (await repository.FindAsync(order.Id)).Value;

            tracked.Name.Should().Be("renamed");
            (await repository.RemoveAsync(tracked)).IsSuccess.Should().BeTrue();
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Should().BeEmpty();
        }
    }

    #endregion

    #region Transactions

    [PostgresFact]
    public async Task Transaction_Rollback_ShouldDiscardChanges()
    {
        await _fixture.ResetAsync();
        var order = Orders.New("first", 10);

        await using (TestContext context = _fixture.CreateContext())
        {
            var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));
            begun.IsSuccess.Should().BeTrue();

            context.Orders.Add(order);
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
            (await begun.Value.RollbackAsync()).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Should().BeEmpty();
        }
    }

    [PostgresFact]
    public async Task Transaction_Commit_ShouldPersistChanges()
    {
        await _fixture.ResetAsync();
        var order = Orders.New("first", 10);

        await using (TestContext context = _fixture.CreateContext())
        {
            var begun = await context.BeginTransactionAsync(TimeSpan.FromSeconds(30));
            begun.IsSuccess.Should().BeTrue();

            context.Orders.Add(order);
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
            (await begun.Value.CommitAsync()).IsSuccess.Should().BeTrue();
        }

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Should().ContainSingle().Which.Id.Should().Be(order.Id);
        }
    }

    #endregion

    #region Soft delete and concurrency

    [PostgresFact]
    public async Task Remove_SoftDeletable_ShouldMarkDeletedAndFilter()
    {
        await _fixture.ResetAsync();
        var order = Orders.Soft("doomed");

        await using TestContext context = _fixture.CreateContext();
        context.SoftOrders.Add(order);
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();

        context.SoftOrders.Remove(order);
        (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();

        order.IsDeleted.Should().BeTrue();
        order.DeletedBy.Should().Be("tester");
        order.DeletedAtUtc.Should().NotBeNull();
        context.SoftOrders.Should().BeEmpty();
        context.SoftOrders.IgnoreQueryFilters().Should().ContainSingle();
    }

    [PostgresFact]
    public async Task SaveChanges_ConcurrencyConflict_ShouldReturnError()
    {
        await _fixture.ResetAsync();
        var order = Orders.Versioned("first");

        await using (TestContext context = _fixture.CreateContext())
        {
            context.VersionedOrders.Add(order);
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await using TestContext first = _fixture.CreateContext();
        await using TestContext second = _fixture.CreateContext();

        VersionedOrder firstCopy = await first.VersionedOrders.SingleAsync(
            candidate => candidate.Id == order.Id);
        VersionedOrder secondCopy = await second.VersionedOrders.SingleAsync(
            candidate => candidate.Id == order.Id);

        firstCopy.Name = "first-writer";
        firstCopy.Version = 2;
        (await first.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();

        secondCopy.Name = "second-writer";
        secondCopy.Version = 2;
        var conflict = await second.SaveChangesAsync(0);

        conflict.IsFailure.Should().BeTrue();
        conflict.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("persistence.concurrency_conflict");
    }

    #endregion

    #region Provider registration

    [PostgresFact]
    public async Task AddPersistencePostgreSql_ShouldWireTheProvider()
    {
        await _fixture.ResetAsync();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BuildingBlocks:Persistence:PostgreSql:ConnectionString"] =
                    _fixture.ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddPersistencePostgreSql<TestContext>(configuration);

        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        TestContext context = scope.ServiceProvider.GetRequiredService<TestContext>();

        (await context.Database.CanConnectAsync()).Should().BeTrue();
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().BeOfType<TestContext>();
    }

    #endregion

    #region Respawn

    [PostgresFact]
    public async Task Reset_ShouldClearTheModuleSchema()
    {
        await _fixture.ResetAsync();

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Add(Orders.New("first", 10));
            (await context.SaveChangesAsync(0)).IsSuccess.Should().BeTrue();
        }

        await _fixture.ResetAsync();

        await using (TestContext context = _fixture.CreateContext())
        {
            context.Orders.Should().BeEmpty();
        }
    }

    #endregion
}
