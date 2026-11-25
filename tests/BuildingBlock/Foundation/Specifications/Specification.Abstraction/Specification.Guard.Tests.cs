namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="SpecificationGuard"/>.</summary>
public sealed class SpecificationGuardTests
{
    [Fact]
    #region Specification

    public void EnsureConcrete_WithSpecificationBase_ShouldSucceed()
    {
        ISpecification<Person> specification = new AdultSpecification();

        var result = SpecificationGuard.EnsureConcrete(
            specification,
            SpecificationConstant.Constraints.Paging.NotSupported.Error);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(specification);
    }

    [Fact]
    public void EnsureConcrete_WithCustomImplementation_ShouldReturnSuppliedError()
    {
        ISpecification<Person> specification = new BareSpecification();

        var result = SpecificationGuard.EnsureConcrete(
            specification,
            SpecificationConstant.Constraints.Paging.NotSupported.Error);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Paging.NotSupported.Error);
    }

    [Fact]
    public void EnsureConcrete_ShouldNotThrow()
    {
        ISpecification<Person> specification = new BareSpecification();

        var act = () => SpecificationGuard.EnsureConcrete(
            specification,
            SpecificationConstant.Constraints.Ordering.NotSupported.Error);

        act.Should().NotThrow();
    }

    [Fact]
    #endregion
    #region Presence

    public void NotNull_WithSpecification_ShouldSucceed()
    {
        ISpecification<Person> specification = new AdultSpecification();

        var result = SpecificationGuard.NotNull(
            specification,
            SpecificationConstant.Constraints.Specification.Required.Error);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(specification);
    }

    [Fact]
    public void NotNull_WithNull_ShouldReturnRequiredError()
    {
        ISpecification<Person>? specification = null;

        var result = SpecificationGuard.NotNull(
            specification,
            SpecificationConstant.Constraints.Specification.Required.Error);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    #endregion
    #region Paging

    public void ValidPage_InRange_ShouldSucceed(int page)
    {
        SpecificationGuard.ValidPage(page).Value.Should().Be(page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidPage_OutOfRange_ShouldReturnError(int page)
    {
        SpecificationGuard.ValidPage(page).Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Paging.Page.Min.Error);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    public void ValidPageSize_InRange_ShouldSucceed(int pageSize)
    {
        SpecificationGuard.ValidPageSize(pageSize).Value.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1001, false)]
    public void ValidPageSize_OutOfRange_ShouldReturnMatchingConstraintError(
        int pageSize,
        bool belowMinimum)
    {
        var expected = belowMinimum
            ? SpecificationConstant.Constraints.Paging.PageSize.Min.Error
            : SpecificationConstant.Constraints.Paging.PageSize.Max.Error;

        SpecificationGuard.ValidPageSize(pageSize).Errors.Should().ContainSingle()
            .Which.Should().Be(expected);
    }

    [Fact]
    public void ValidPaging_ShouldComputeSkipAndTake()
    {
        var result = SpecificationGuard.ValidPaging(page: 3, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Skip.Should().Be(40);
        result.Value.Take.Should().Be(20);
    }

    [Fact]
    public void ValidPaging_InvalidInput_ShouldReturnErrorWithoutThrowing()
    {
        var act = () => SpecificationGuard.ValidPaging(page: 0, pageSize: 0);

        act.Should().NotThrow().Which.IsFailure.Should().BeTrue();
    }

    [Fact]
    #endregion
    #region Factory Inputs

    public void CriteriaRequired_ShouldGuardAgainstNull()
    {
        System.Linq.Expressions.Expression<Func<Person, bool>>? criteria = null;

        var result = SpecificationGuard.CriteriaRequired(criteria);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Criteria.Required.Error);
    }

    [Fact]
    public void CriteriaRequired_ShouldPassValidExpression()
    {
        System.Linq.Expressions.Expression<Func<Person, bool>> criteria = person => person.Age > 0;

        SpecificationGuard.CriteriaRequired(criteria).Value.Should().BeSameAs(criteria);
    }

    [Fact]
    public void IdRequired_ShouldGuardAgainstDefaultId()
    {
        SpecificationGuard.IdRequired(Guid.Empty).Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Id.Required.Error);
    }

    [Fact]
    public void IdsRequired_ShouldGuardAgainstNullOrEmpty()
    {
        SpecificationGuard.IdsRequired<Guid>(null!).Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ids.Required.Error);
        SpecificationGuard.IdsRequired(Array.Empty<Guid>()).Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ids.Required.Error);
    }
    #endregion
}
