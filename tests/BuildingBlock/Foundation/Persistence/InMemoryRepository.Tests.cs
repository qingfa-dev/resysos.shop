namespace BuildingBlock.Persistence.Unit.Tests;

/// <summary>Tests for <see cref="InMemoryRepository{TAggregate, TId}"/>.</summary>
public sealed class InMemoryRepositoryTests
{
    #region Add

    [Fact]
    public async Task AddAsync_ShouldSucceed()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);

        var result = await repository.AddAsync(order);

        result.IsSuccess.Should().BeTrue();
        (await repository.FindAsync(order.Id)).Value.Should().BeSameAs(order);
    }

    [Fact]
    public async Task AddAsync_NullAggregate_ShouldReturnError()
    {
        var repository = Orders.Repository();

        var result = await repository.AddAsync(null!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.aggregate.required");
    }

    [Fact]
    public async Task AddAsync_DefaultId_ShouldReturnError()
    {
        var repository = Orders.Repository();

        var result = await repository.AddAsync(Orders.WithId(Guid.Empty, "x", 1));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.id.required");
    }

    [Fact]
    public async Task AddAsync_Duplicate_ShouldReturnConflictError()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        await repository.AddAsync(order);

        var result = await repository.AddAsync(order);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.already_exists");
    }

    #endregion

    #region Find

    [Fact]
    public async Task FindAsync_ExistingAggregate_ShouldSucceed()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        await repository.AddAsync(order);

        (await repository.FindAsync(order.Id)).Value.Should().BeSameAs(order);
    }

    [Fact]
    public async Task FindAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = Orders.Repository();

        var result = await repository.FindAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    [Fact]
    public async Task FindAsync_DefaultId_ShouldReturnError()
    {
        var repository = Orders.Repository();

        var result = await repository.FindAsync(Guid.Empty);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.id.required");
    }

    #endregion

    #region Update and Remove

    [Fact]
    public async Task UpdateAsync_ExistingAggregate_ShouldReplaceValue()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        await repository.AddAsync(order);
        var updated = Orders.WithId(order.Id, "renamed", 20);

        var result = await repository.UpdateAsync(updated);

        result.IsSuccess.Should().BeTrue();
        (await repository.FindAsync(order.Id)).Value!.Name.Should().Be("renamed");
    }

    [Fact]
    public async Task UpdateAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = Orders.Repository();

        var result = await repository.UpdateAsync(Orders.New("ghost", 1));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    [Fact]
    public async Task RemoveAsync_ExistingAggregate_ShouldSucceed()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        await repository.AddAsync(order);

        var result = await repository.RemoveAsync(order);

        result.IsSuccess.Should().BeTrue();
        (await repository.ExistsAsync(order.Id)).Value.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_MissingAggregate_ShouldReturnNotFoundError()
    {
        var repository = Orders.Repository();

        var result = await repository.RemoveAsync(Orders.New("ghost", 1));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.not_found");
    }

    #endregion

    #region Queries

    [Fact]
    public async Task ExistsAsync_ShouldReflectPresence()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        await repository.AddAsync(order);

        (await repository.ExistsAsync(order.Id)).Value.Should().BeTrue();
        (await repository.ExistsAsync(Guid.NewGuid())).Value.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_ShouldApplyPredicate()
    {
        var repository = Orders.Repository();
        await repository.AddAsync(Orders.New("small", 10));
        await repository.AddAsync(Orders.New("large", 100));

        var result = await repository.ListAsync(order => order.Amount >= 50);

        result.Value.Should().ContainSingle().Which.Name.Should().Be("large");
    }

    [Fact]
    public async Task ListAsync_NullPredicate_ShouldReturnError()
    {
        var repository = Orders.Repository();

        var result = await repository.ListAsync(null!);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.predicate.required");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchOrNotFound()
    {
        var repository = Orders.Repository();
        await repository.AddAsync(Orders.New("small", 10));
        await repository.AddAsync(Orders.New("large", 100));

        (await repository.FirstOrDefaultAsync(order => order.Amount >= 50)).Value!.Name.Should().Be("large");

        var missing = await repository.FirstOrDefaultAsync(order => order.Amount > 1000);
        missing.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("in_memory_repository.no_match");
    }

    [Fact]
    public async Task CountAsync_ShouldCountMatches()
    {
        var repository = Orders.Repository();
        await repository.AddAsync(Orders.New("a", 10));
        await repository.AddAsync(Orders.New("b", 20));
        await repository.AddAsync(Orders.New("c", 30));

        (await repository.CountAsync(order => order.Amount >= 20)).Value.Should().Be(2);
    }

    #endregion

    #region Seeding

    [Fact]
    public async Task Seed_ShouldOverwriteAndClear_ShouldEmptyStore()
    {
        var repository = Orders.Repository();
        var order = Orders.New("first", 10);
        repository.Seed(order).IsSuccess.Should().BeTrue();

        // Seeding the same id again replaces without a conflict.
        repository.Seed(Orders.WithId(order.Id, "replaced", 99)).IsSuccess.Should().BeTrue();
        (await repository.CountAsync(_ => true)).Value.Should().Be(1);

        repository.Clear().IsSuccess.Should().BeTrue();
        (await repository.CountAsync(_ => true)).Value.Should().Be(0);
    }

    #endregion
}
