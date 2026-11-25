using BuildingBlock.Specification.Abstraction.Unit.Tests;

namespace BuildingBlock.Specification.Abstraction.Unit.Tests;

/// <summary>Tests for <see cref="ISpecification{T}"/> implementations.</summary>
public sealed class SpecificationInterfaceTests
{
    #region Contract

    [Fact]
    public void SpecificationBase_ShouldExposeTheFullContract()
    {
        ISpecification<Person> specification = new AdultSpecification();

        specification.Criteria.Should().NotBeNull();
        specification.Includes.Should().BeEmpty();
        specification.IncludeStrings.Should().BeEmpty();
        specification.OrderBy.Should().BeEmpty();
        specification.Skip.Should().BeNull();
        specification.Take.Should().BeNull();
        specification.IsPagingEnabled.Should().BeFalse();
        specification.IsTrackingEnabled.Should().BeTrue();
        specification.IsSatisfiedBy(People.Adult).Should().BeTrue();
    }

    [Fact]
    public void CustomImplementation_ShouldNotNeedTheBaseClass()
    {
        ISpecification<Person> specification = new BareSpecification();

        specification.IsSatisfiedBy(People.Child).Should().BeTrue();
        specification.Compile().Value(People.Child).Should().BeTrue();
    }
    #endregion
}
