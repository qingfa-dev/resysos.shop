namespace BuildingBlock.Persistence.Unit.Tests;

/// <summary>Tests for <see cref="RepositoryGuard"/>.</summary>
public sealed class RepositoryGuardTests
{
    #region EnsureFound

    [Fact]
    public void EnsureFound_ShouldPassAggregate()
    {
        var order = Orders.New("first", 10);

        RepositoryGuard.EnsureFound<Order, Guid>(order, order.Id).Value.Should().BeSameAs(order);
    }

    [Fact]
    public void EnsureFound_ShouldGuardNull()
    {
        var id = Guid.NewGuid();

        var result = RepositoryGuard.EnsureFound<Order, Guid>(null, id);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("repository.not_found");
    }

    #endregion

    #region Validation

    [Fact]
    public void NotNull_ShouldGuardNull()
        => RepositoryGuard.NotNull<Order>(null).Errors.Should().ContainSingle()
            .Which.Code.Should().Be("repository.aggregate.required");

    [Fact]
    public void PredicateRequired_ShouldGuardNull()
        => RepositoryGuard.PredicateRequired<Order>(null).Errors.Should().ContainSingle()
            .Which.Code.Should().Be("repository.predicate.required");

    [Fact]
    public void PredicateRequired_ShouldPassPredicate()
        => RepositoryGuard.PredicateRequired<Order>(order => order.Amount > 0).IsSuccess.Should().BeTrue();

    #endregion

    #region Identifiers

    [Fact]
    public void ValidId_ShouldGuardDefaultId()
        => RepositoryGuard.ValidId(Guid.Empty).Errors.Should().ContainSingle()
            .Which.Code.Should().Be("repository.id.required");

    [Fact]
    public void ValidId_ShouldPassValidId()
    {
        var id = Guid.NewGuid();

        RepositoryGuard.ValidId(id).Value.Should().Be(id);
    }

    #endregion

    #region Paging

    [Fact]
    public void ValidPaging_ShouldComputeSkipAndTake()
    {
        var paging = RepositoryGuard.ValidPaging(3, 20).Value;

        paging.Skip.Should().Be(40);
        paging.Take.Should().Be(20);
    }

    [Fact]
    public void ValidPaging_BelowMinimumPage_ShouldFail()
        => RepositoryGuard.ValidPaging(0, 20).IsFailure.Should().BeTrue();

    [Fact]
    public void ValidPaging_BelowMinimumPageSize_ShouldFail()
        => RepositoryGuard.ValidPaging(1, 0).IsFailure.Should().BeTrue();

    [Fact]
    public void ValidPaging_AboveMaximumPageSize_ShouldFail()
        => RepositoryGuard.ValidPaging(1, RepositoryConstant.Constraints.Paging.PageSize.Max.Value + 1)
            .IsFailure.Should().BeTrue();

    #endregion
}
