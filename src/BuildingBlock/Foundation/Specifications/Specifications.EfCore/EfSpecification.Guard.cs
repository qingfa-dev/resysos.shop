using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace BuildingBlock.Specifications.EfCore;

/// <summary>Guards for the EF Core specification provider. Failures stay on the railway.</summary>
public static class EfSpecificationGuard
{
    #region Query

    /// <summary>Checks that a source query was supplied.</summary>
    public static Result<IQueryable<T>> QueryRequired<T>(IQueryable<T>? query)
        => query is null
            ? Result.Failure<IQueryable<T>>(EfSpecificationConstant.Query.Required.Error)
            : Result.Success(query);

    #endregion

    #region Specification

    /// <summary>Checks that a specification was supplied.</summary>
    public static Result<ISpecification<T>> SpecificationRequired<T>(ISpecification<T>? specification)
        => specification is null
            ? Result.Failure<ISpecification<T>>(
                EfSpecificationConstant.Specification.Required.Error)
            : Result.Success(specification);

    #endregion

    #region Evaluator

    /// <summary>Checks that an evaluator was supplied.</summary>
    public static Result<IEfSpecificationEvaluator> EvaluatorRequired(
        IEfSpecificationEvaluator? evaluator)
        => evaluator is null
            ? Result.Failure<IEfSpecificationEvaluator>(
                EfSpecificationConstant.Evaluator.Required.Error)
            : Result.Success(evaluator);

    #endregion

    #region Options

    /// <summary>Checks that options were supplied and unwraps their value.</summary>
    public static Result<EfSpecificationOptions> OptionsRequired(
        IOptions<EfSpecificationOptions>? options)
        => options is null
            ? Result.Failure<EfSpecificationOptions>(
                EfSpecificationConstant.Options.Required.Error)
            : Result.Success(options.Value);

    #endregion

    #region Services

    /// <summary>Checks that a service collection was supplied.</summary>
    public static Result<IServiceCollection> ServicesRequired(IServiceCollection? services)
        => services is null
            ? Result.Failure<IServiceCollection>(
                EfSpecificationConstant.Services.Required.Error)
            : Result.Success(services);

    #endregion
}
