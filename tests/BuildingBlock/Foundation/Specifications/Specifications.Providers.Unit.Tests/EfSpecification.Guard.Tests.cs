using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="EfSpecificationGuard"/>.</summary>
public sealed class EfSpecificationGuardTests
{
    #region Query

    [Fact]
    public void QueryRequired_ShouldGuardNull()
    {
        IQueryable<Person>? query = null;

        EfSpecificationGuard.QueryRequired(query).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Query.Required.Error);
    }

    [Fact]
    public void QueryRequired_ShouldPassValidQuery()
    {
        var query = People.Query(People.Adult());

        EfSpecificationGuard.QueryRequired(query).Value.Should().BeSameAs(query);
    }

    #endregion

    #region Specification

    [Fact]
    public void SpecificationRequired_ShouldGuardNull()
    {
        ISpecification<Person>? specification = null;

        EfSpecificationGuard.SpecificationRequired(specification).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Specification.Required.Error);
    }

    #endregion

    #region Evaluator

    [Fact]
    public void EvaluatorRequired_ShouldGuardNull()
    {
        IEfSpecificationEvaluator? evaluator = null;

        EfSpecificationGuard.EvaluatorRequired(evaluator).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Evaluator.Required.Error);
    }

    #endregion

    #region Options

    [Fact]
    public void OptionsRequired_ShouldGuardNull()
    {
        IOptions<EfSpecificationOptions>? options = null;

        EfSpecificationGuard.OptionsRequired(options).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Options.Required.Error);
    }

    [Fact]
    public void OptionsRequired_ShouldUnwrapValue()
    {
        var options = Options.Create(new EfSpecificationOptions { UseSplitQuery = true });

        EfSpecificationGuard.OptionsRequired(options).Value.UseSplitQuery.Should().BeTrue();
    }

    #endregion

    #region Services

    [Fact]
    public void ServicesRequired_ShouldGuardNull()
    {
        IServiceCollection? services = null;

        EfSpecificationGuard.ServicesRequired(services).Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Services.Required.Error);
    }

    #endregion
}
