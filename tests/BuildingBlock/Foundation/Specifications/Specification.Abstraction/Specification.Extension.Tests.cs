namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for the Result-returning <see cref="SpecificationExtensions"/> factories.</summary>
public sealed class SpecificationExtensionTests
{
    #region Evaluation

    [Fact]
    public void Compile_ShouldReturnReusableDelegate()
    {
        var specification = new AdultSpecification();

        var result = specification.Compile();

        result.IsSuccess.Should().BeTrue();
        result.Value(People.Adult).Should().BeTrue();
        result.Value(People.Child).Should().BeFalse();
    }

    #endregion
    #region Factories

    [Fact]
    public void Create_ShouldBuildAdHocSpecification()
    {
        var result = SpecificationExtensions.Create<Person>(person => person.Age < 18);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSatisfiedBy(People.Child).Should().BeTrue();
        result.Value.IsSatisfiedBy(People.Adult).Should().BeFalse();
    }

    [Fact]
    public void ById_ShouldMatchIdentifier()
    {
        var person = People.Adult;

        var result = SpecificationExtensions.ById<Person, Guid>(person.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSatisfiedBy(person).Should().BeTrue();
        result.Value.IsSatisfiedBy(People.Other).Should().BeFalse();
    }

    [Fact]
    public void ByIds_ShouldMatchAnyIdentifier()
    {
        var person = People.Adult;
        var other = People.Other;

        var result = SpecificationExtensions.ByIds<Person, Guid>(
            new[] { person.Id, other.Id });

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSatisfiedBy(person).Should().BeTrue();
        result.Value.IsSatisfiedBy(other).Should().BeTrue();
        result.Value.IsSatisfiedBy(People.Child).Should().BeFalse();
    }

    #endregion
    #region Invalid Input

    [Fact]
    public void ByIds_EmptyIds_ShouldReturnError()
    {
        var result = SpecificationExtensions.ByIds<Person, Guid>(Array.Empty<Guid>());

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ids.Required.Error);
    }

    [Fact]
    public void Create_NullCriteria_ShouldReturnError()
    {
        var result = SpecificationExtensions.Create<Person>(null!);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Criteria.Required.Error);
    }

    [Fact]
    public void ById_DefaultId_ShouldReturnError()
    {
        var result = SpecificationExtensions.ById<Person, Guid>(Guid.Empty);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Id.Required.Error);
    }

    [Fact]
    public void ByIds_NullIds_ShouldReturnError()
    {
        var result = SpecificationExtensions.ByIds<Person, Guid>(null!);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Ids.Required.Error);
    }

    [Fact]
    public void Compile_NullSpecification_ShouldReturnError()
    {
        ISpecification<Person>? specification = null;

        var result = specification!.Compile();

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
    }
    #endregion
}
