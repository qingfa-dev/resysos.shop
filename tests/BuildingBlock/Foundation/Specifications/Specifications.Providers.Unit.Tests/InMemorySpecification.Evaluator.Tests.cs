namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="InMemorySpecificationEvaluator"/>.</summary>
public sealed class InMemorySpecificationEvaluatorTests
{
    #region Factory

    [Fact]
    public void Create_ShouldSucceed()
    {
        InMemorySpecificationEvaluator.Create().IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Validation

    [Fact]
    public void Apply_NullSource_ShouldReturnError()
    {
        var evaluator = InMemorySpecificationEvaluator.Create().Value;
        var specification = SpecificationExtensions.Create<Person>(_ => true).Value;

        evaluator.Apply(null!, specification).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Source.Required.Error);
    }

    [Fact]
    public void Apply_NullSpecification_ShouldReturnError()
    {
        var evaluator = InMemorySpecificationEvaluator.Create().Value;

        evaluator.Apply(Array.Empty<Person>(), null!).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Specification.Required.Error);
    }

    #endregion

    #region Evaluation

    [Fact]
    public void Apply_ShouldFilterByCriteria()
    {
        var evaluator = InMemorySpecificationEvaluator.Create().Value;
        var specification = SpecificationExtensions
            .Create<Person>(person => person.Age >= 20)
            .Value;

        var result = evaluator.Apply(
            [People.Adult(10), People.Adult(20), People.Adult(30)],
            specification).Value.ToList();

        result.Select(person => person.Age).Should().Equal(20, 30);
    }

    [Fact]
    public void Apply_ShouldOrderByMultipleKeys()
    {
        var evaluator = InMemorySpecificationEvaluator.Create().Value;
        var specification = SpecificationExtensions.Create<Person>(_ => true).Value
            .OrderBy(person => person.Age).Value
            .OrderBy(person => person.Name).Value;
        var people = new[]
        {
            new Person { Id = Guid.NewGuid(), Name = "B", Age = 20 },
            new Person { Id = Guid.NewGuid(), Name = "A", Age = 20 },
            new Person { Id = Guid.NewGuid(), Name = "C", Age = 10 },
        };

        var result = evaluator.Apply(people, specification).Value.ToList();

        result.Select(person => person.Name).Should().Equal("C", "A", "B");
    }

    [Fact]
    public void Apply_ShouldPage()
    {
        var evaluator = InMemorySpecificationEvaluator.Create().Value;
        var specification = SpecificationExtensions.Create<Person>(_ => true).Value
            .WithPaging(page: 2, pageSize: 2).Value;

        var result = evaluator.Apply(
            [People.Adult(10), People.Adult(20), People.Adult(30), People.Adult(40)],
            specification).Value.ToList();

        result.Select(person => person.Age).Should().Equal(30, 40);
    }

    #endregion
}
