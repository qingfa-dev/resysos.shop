namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for the Result-returning And/Or/Not composite factories.</summary>
public sealed class SpecificationCompositeTests
{
    #region Composition

    [Fact]
    public void And_ShouldRequireBothSpecifications()
    {
        var result = new AdultSpecification().And(new NamedAdaSpecification());

        result.IsSuccess.Should().BeTrue();
        var specification = result.Value;
        specification.IsSatisfiedBy(People.Adult).Should().BeTrue();
        specification.IsSatisfiedBy(People.Other).Should().BeFalse();
        specification.IsSatisfiedBy(People.Child).Should().BeFalse();
    }

    [Fact]
    public void Or_ShouldRequireEitherSpecification()
    {
        var result = new AdultSpecification().Or(new NamedAdaSpecification());

        result.IsSuccess.Should().BeTrue();
        var specification = result.Value;
        specification.IsSatisfiedBy(People.Adult).Should().BeTrue();
        specification.IsSatisfiedBy(People.Other).Should().BeTrue();
        specification.IsSatisfiedBy(People.Child).Should().BeTrue();
    }

    [Fact]
    public void Not_ShouldNegateSpecification()
    {
        var result = new AdultSpecification().Not();

        result.IsSuccess.Should().BeTrue();
        var specification = result.Value;
        specification.IsSatisfiedBy(People.Adult).Should().BeFalse();
        specification.IsSatisfiedBy(People.Child).Should().BeTrue();
    }

    [Fact]
    public void Operators_ShouldComposeLikeMethods()
    {
        var and = new AdultSpecification() & new NamedAdaSpecification();
        var or = new AdultSpecification() | new NamedAdaSpecification();
        var not = !new AdultSpecification();

        and.IsSuccess.Should().BeTrue();
        or.IsSuccess.Should().BeTrue();
        not.IsSuccess.Should().BeTrue();

        and.Value.IsSatisfiedBy(People.Adult).Should().BeTrue();
        and.Value.IsSatisfiedBy(People.Other).Should().BeFalse();
        or.Value.IsSatisfiedBy(People.Other).Should().BeTrue();
        not.Value.IsSatisfiedBy(People.Child).Should().BeTrue();
    }

    #endregion
    #region Invalid Input

    [Fact]
    public void Composition_NullOperand_ShouldReturnErrorWithoutThrowing()
    {
        var left = new AdultSpecification();

        Specification<Person>? nullSpec = null;

        var and = left.And(null!);
        var or = left.Or(null!);
        var not = !nullSpec!;

        and.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
        or.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
        not.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
    }

    [Fact]
    public void Composition_NullLeftOperand_ShouldReturnError()
    {
        Specification<Person>? left = null;

        var and = left! & new NamedAdaSpecification();
        var or = left! | new NamedAdaSpecification();
        var not = !left!;

        and.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
        or.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
        not.Errors.Should().ContainSingle()
            .Which.Should().Be(SpecificationConstant.Constraints.Specification.Required.Error);
    }
    #endregion
}
