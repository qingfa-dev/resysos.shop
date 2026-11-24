namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="RepositoryGuard"/>.</summary>
public sealed class RepositoryGuardTests
{
    [Fact]
    #region Lookups

    public void EnsureFound_WithAggregate_ShouldSucceed()
    {
        var aggregate = People.Adult;

        var result = RepositoryGuard.EnsureFound<Person, Guid>(aggregate, aggregate.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(aggregate);
    }

    [Fact]
    public void EnsureFound_WithoutAggregate_ShouldReturnNotFoundError()
    {
        var id = Guid.NewGuid();

        var result = RepositoryGuard.EnsureFound<Person, Guid>(null, id);

        result.IsFailure.Should().BeTrue();
        var error = result.Errors.Should().ContainSingle().Which;
        error.Code.Should().Be(RepositoryConstant.Constraints.NotFound.Error.Code);
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Be($"{nameof(Person)} with id '{id}' was not found.");
    }

    [Fact]
    public void EnsureFound_ShouldNotThrow()
    {
        var act = () => RepositoryGuard.EnsureFound<Person, Guid>(null, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    #endregion
    #region Validation

    public void NotNull_WithAggregate_ShouldSucceed()
    {
        var aggregate = People.Adult;

        RepositoryGuard.NotNull(aggregate).Value.Should().BeSameAs(aggregate);
    }

    [Fact]
    public void NotNull_WithNull_ShouldReturnRequiredError()
    {
        RepositoryGuard.NotNull<Person>(null).Errors.Should().ContainSingle()
            .Which.Should().Be(RepositoryConstant.Constraints.Aggregate.Required.Error);
    }

    [Fact]
    public void ValidId_WithNonNullId_ShouldSucceed()
    {
        var id = Guid.NewGuid();

        RepositoryGuard.ValidId(id).Value.Should().Be(id);
    }

    [Fact]
    public void ValidId_WithDefaultId_ShouldReturnError()
    {
        RepositoryGuard.ValidId(Guid.Empty).Errors.Should().ContainSingle()
            .Which.Should().Be(RepositoryConstant.Constraints.Id.Required.Error);
    }

    [Fact]
    #endregion
    #region Paging

    public void ValidPaging_ShouldComputeSkipAndTake()
    {
        var result = RepositoryGuard.ValidPaging(page: 3, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Skip.Should().Be(40);
        result.Value.Take.Should().Be(20);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1001, 2)]
    public void ValidPaging_InvalidInput_ShouldReturnMatchingConstraintError(
        int page,
        int pageSize,
        int expectedError)
    {
        Error[] expectedErrors =
        [
            RepositoryConstant.Constraints.Paging.Page.Min.Error,
            RepositoryConstant.Constraints.Paging.PageSize.Min.Error,
            RepositoryConstant.Constraints.Paging.PageSize.Max.Error,
        ];

        var result = RepositoryGuard.ValidPaging(page, pageSize);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(expectedErrors[expectedError]);
    }
    #endregion
}
