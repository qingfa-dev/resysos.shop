namespace BuildingBlock.Specifications.InMemory;

/// <summary>Guards for the in-memory provider. Failures stay on the railway.</summary>
public static class InMemorySpecificationGuard
{
    #region Query

    /// <summary>Checks that a source sequence was supplied.</summary>
    public static Result<IEnumerable<T>> SourceRequired<T>(IEnumerable<T>? source)
        => source is null
            ? Result.Failure<IEnumerable<T>>(InMemorySpecificationConstant.Source.Required.Error)
            : Result.Success(source);

    #endregion

    #region Specification

    /// <summary>Checks that a specification was supplied.</summary>
    public static Result<ISpecification<T>> SpecificationRequired<T>(
        ISpecification<T>? specification)
        => specification is null
            ? Result.Failure<ISpecification<T>>(
                InMemorySpecificationConstant.Specification.Required.Error)
            : Result.Success(specification);

    #endregion

    #region Evaluator

    /// <summary>Checks that an evaluator was supplied.</summary>
    public static Result<IInMemorySpecificationEvaluator> EvaluatorRequired(
        IInMemorySpecificationEvaluator? evaluator)
        => evaluator is null
            ? Result.Failure<IInMemorySpecificationEvaluator>(
                InMemorySpecificationConstant.Evaluator.Required.Error)
            : Result.Success(evaluator);

    #endregion

    #region Aggregate

    /// <summary>Checks that an aggregate was supplied.</summary>
    public static Result<TAggregate> AggregateRequired<TAggregate>(TAggregate? aggregate)
        where TAggregate : class
        => aggregate is null
            ? Result.Failure<TAggregate>(InMemorySpecificationConstant.Aggregate.Required.Error)
            : Result.Success(aggregate);

    #endregion

    #region Identifier

    /// <summary>Checks that an identifier is not the default value of its type.</summary>
    public static Result<TId> IdRequired<TId>(TId id)
        where TId : notnull
        => EqualityComparer<TId>.Default.Equals(id, default!)
            ? Result.Failure<TId>(InMemorySpecificationConstant.Id.Required.Error)
            : Result.Success(id);

    #endregion
}
