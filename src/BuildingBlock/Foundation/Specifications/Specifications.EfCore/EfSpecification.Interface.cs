namespace BuildingBlock.Specifications.EfCore;

/// <summary>Turns a specification into an EF Core query.</summary>
public interface IEfSpecificationEvaluator
{
    /// <summary>
    /// Applies criteria, includes, ordering, paging and tracking settings to a query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="specification">The specification.</param>
    /// <returns>The translated query, or validation errors.</returns>
    Result<IQueryable<T>> Apply<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class;
}
