namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="InMemorySpecificationGuard"/>.</summary>
public sealed class InMemorySpecificationGuardTests
{
    #region Source

    [Fact]
    public void SourceRequired_ShouldGuardNull()
    {
        IEnumerable<Person>? source = null;

        InMemorySpecificationGuard.SourceRequired(source).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Source.Required.Error);
    }

    #endregion

    #region Specification

    [Fact]
    public void SpecificationRequired_ShouldGuardNull()
    {
        ISpecification<Person>? specification = null;

        InMemorySpecificationGuard.SpecificationRequired(specification).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Specification.Required.Error);
    }

    #endregion

    #region Evaluator

    [Fact]
    public void EvaluatorRequired_ShouldGuardNull()
    {
        IInMemorySpecificationEvaluator? evaluator = null;

        InMemorySpecificationGuard.EvaluatorRequired(evaluator).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Evaluator.Required.Error);
    }

    #endregion

    #region Aggregate

    [Fact]
    public void AggregateRequired_ShouldGuardNull()
    {
        InMemorySpecificationGuard.AggregateRequired<Person>(null).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Aggregate.Required.Error);
    }

    #endregion

    #region Identifier

    [Fact]
    public void IdRequired_ShouldGuardDefaultId()
    {
        InMemorySpecificationGuard.IdRequired(Guid.Empty).Errors.Should().ContainSingle()
            .Which.Should().Be(InMemorySpecificationConstant.Id.Required.Error);
    }

    [Fact]
    public void IdRequired_ShouldPassValidId()
    {
        var id = Guid.NewGuid();

        InMemorySpecificationGuard.IdRequired(id).Value.Should().Be(id);
    }

    #endregion
}
