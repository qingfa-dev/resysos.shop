using System.Linq.Expressions;

namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="SpecificationQueryExtensions"/>.</summary>
public sealed class SpecificationQueryExtensionTests
{
    #region Paging

    [Fact]
    public void WithPaging_ShouldSetSkipAndTake()
    {
        var specification = new AdultSpecification();

        var result = specification.WithPaging(page: 2, pageSize: 10);

        result.IsSuccess.Should().BeTrue();
        var shaped = result.Value;
        shaped.Should().BeSameAs(specification);
        shaped.IsPagingEnabled.Should().BeTrue();
        shaped.Skip.Should().Be(10);
        shaped.Take.Should().Be(10);
    }

    [Fact]
    public void WithPaging_Defaults_ShouldUseConstantValues()
    {
        var specification = new AdultSpecification();

        var result = specification.WithPaging();

        result.IsSuccess.Should().BeTrue();
        result.Value.Skip.Should().Be(0);
        result.Value.Take.Should().Be(SpecificationConstant.Defaults.PageSize);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 1001)]
    public void WithPaging_InvalidInput_ShouldReturnConstraintError(int page, int pageSize)
    {
        ISpecification<Person> specification = new AdultSpecification();

        var act = () => specification.WithPaging(page, pageSize);

        var result = act.Should().NotThrow().Which;
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Ordering

    [Fact]
    public void OrderBy_ShouldRecordAscendingOrdering()
    {
        var specification = new AdultSpecification();

        var result = specification.OrderBy(person => person.Age);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderBy.Should().ContainSingle();
        result.Value.OrderBy[0].Descending.Should().BeFalse();
    }

    [Fact]
    public void OrderBy_Descending_ShouldRecordDescendingOrdering()
    {
        var specification = new AdultSpecification();

        var result = specification.OrderBy(person => person.Age, descending: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderBy.Should().ContainSingle();
        result.Value.OrderBy[0].Descending.Should().BeTrue();
    }

    [Fact]
    public void OrderBy_NullKeySelector_ShouldReturnErrorWithoutThrowing()
    {
        var specification = new AdultSpecification();

        var act = () => specification.OrderBy((Expression<Func<Person, int>>)null!);

        var result = act.Should().NotThrow().Which;
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ordering.KeySelector.Required.Error);
    }

    #endregion

    #region Tracking

    [Fact]
    public void AsNoTracking_ShouldDisableTracking()
    {
        var specification = new AdultSpecification();

        var result = specification.AsNoTracking();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsTrackingEnabled.Should().BeFalse();
    }

    #endregion

    #region Invalid Input

    [Fact]
    public void Shaping_CustomImplementation_ShouldReturnErrorsInsteadOfThrowing()
    {
        ISpecification<Person> specification = new BareSpecification();

        var paging = specification.WithPaging(1, 10);
        var ordering = specification.OrderBy(person => person.Age);
        var tracking = specification.AsNoTracking();

        paging.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Paging.NotSupported.Error);
        ordering.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ordering.NotSupported.Error);
        tracking.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Tracking.NotSupported.Error);
    }

    [Fact]
    public void Shaping_CustomImplementation_ShouldNotThrow()
    {
        ISpecification<Person> specification = new BareSpecification();

        var paging = () => specification.WithPaging(1, 10);
        var ordering = () => specification.OrderBy(person => person.Age);
        var tracking = () => specification.AsNoTracking();

        paging.Should().NotThrow();
        ordering.Should().NotThrow();
        tracking.Should().NotThrow();
    }

    [Fact]
    public void Shaping_NullSpecification_ShouldReturnRequiredError()
    {
        ISpecification<Person>? specification = null;

        var result = specification!.WithPaging(1, 10);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
    }

    #endregion
}
