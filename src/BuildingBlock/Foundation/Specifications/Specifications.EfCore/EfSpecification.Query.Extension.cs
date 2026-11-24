
namespace BuildingBlock.Specifications.EfCore;

/// <summary>Queryable helpers built on top of the EF Core evaluator.</summary>
public static class EfSpecificationQueryExtensions
{
    #region Evaluation

    /// <summary>Applies a specification to a query using the supplied evaluator.</summary>
    public static Result<IQueryable<T>> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> specification,
        IEfSpecificationEvaluator evaluator)
        where T : class
        => EfSpecificationGuard.EvaluatorRequired(evaluator)
            .Bind(valid => valid.Apply(query, specification));

    #endregion
}
