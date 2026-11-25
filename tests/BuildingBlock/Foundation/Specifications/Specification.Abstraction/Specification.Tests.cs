namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="Specification{T}"/>.</summary>
public sealed class SpecificationTests
{
    #region State

    [Fact]
    public void Criteria_ShouldBeExposedAsExpression()
    {
        var specification = new AdultSpecification();

        specification.Criteria.Should().NotBeNull();
        specification.Criteria.ToString().Should().Contain("Age");
    }

    [Fact]
    public void IsSatisfiedBy_ShouldEvaluateCriteria()
    {
        var specification = new AdultSpecification();

        specification.IsSatisfiedBy(People.Adult).Should().BeTrue();
        specification.IsSatisfiedBy(People.Child).Should().BeFalse();
    }

    [Fact]
    public void Defaults_ShouldBeEmptyAndTrackingEnabled()
    {
        var specification = new AdultSpecification();

        specification.Includes.Should().BeEmpty();
        specification.IncludeStrings.Should().BeEmpty();
        specification.OrderBy.Should().BeEmpty();
        specification.Skip.Should().BeNull();
        specification.Take.Should().BeNull();
        specification.IsPagingEnabled.Should().BeFalse();
        specification.IsTrackingEnabled.Should().BeTrue();
    }

    #endregion
    #region Mutators

    [Fact]
    public void AddInclude_ShouldRecordExpressionAndString()
    {
        var specification = new ShapedSpecification();

        specification.AddIncludePath(person => person.Name);
        specification.AddIncludePath("Address");

        specification.Includes.Should().ContainSingle();
        specification.IncludeStrings.Should().ContainSingle().Which.Should().Be("Address");
    }

    [Fact]
    public void AddOrderBy_ShouldRecordDirection()
    {
        var specification = new ShapedSpecification();

        specification.AddAscending(person => person.Age);
        specification.AddDescending(person => person.Name);

        specification.OrderBy.Should().HaveCount(2);
        specification.OrderBy[0].Descending.Should().BeFalse();
        specification.OrderBy[1].Descending.Should().BeTrue();
    }

    [Fact]
    public void ApplyPaging_ShouldSetSkipTakeAndEnablePaging()
    {
        var specification = new ShapedSpecification();

        specification.Page(20, 10);

        specification.Skip.Should().Be(20);
        specification.Take.Should().Be(10);
        specification.IsPagingEnabled.Should().BeTrue();
    }

    [Fact]
    public void DisableTracking_ShouldTurnTrackingOff()
    {
        var specification = new ShapedSpecification();

        specification.NoTracking();

        specification.IsTrackingEnabled.Should().BeFalse();
    }

    #endregion
    #region Construction

    [Fact]
    public void BaseConstructor_ShouldBeProtected()
    {
        var type = typeof(Specification<Person>);

        type.GetConstructors().Should().BeEmpty();
        type.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                Type.EmptyTypes,
                modifiers: null)
            .Should().NotBeNull();
    }

    #endregion
    #region Factory API

    [Theory]
    [InlineData("AndSpecification`1")]
    [InlineData("OrSpecification`1")]
    [InlineData("NotSpecification`1")]
    [InlineData("AdHocSpecification`1")]
    public void InternalSpecificationTypes_ShouldNotExposeConstructors(string typeName)
    {
        var type = typeof(Specification<Person>).Assembly.GetTypes()
            .Single(candidate => candidate.Name == typeName);

        type.GetConstructors().Should().BeEmpty();
        type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .Should().Contain("Create");
    }
    #endregion
}
