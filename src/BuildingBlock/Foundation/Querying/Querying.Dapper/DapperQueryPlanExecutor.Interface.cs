namespace BuildingBlock.Querying.Dapper;

/// <summary>
/// Applies a compiled query plan to a Dapper source. Dapper returns materialized rows, so the
/// executor works over an <see cref="IEnumerable{T}"/> as well as an <see cref="IQueryable{T}"/>.
/// </summary>
public interface IDapperQueryPlanExecutor : IQueryPlanExecutor
{
    #region Build

    /// <summary>Builds the executable query for a plan over a materialized sequence.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The sequence the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <returns>The composed query, or a validation error.</returns>
    Result<IEnumerable<T>> Build<T>(IEnumerable<T> source, QueryPlan<T> plan) where T : class;

    #endregion

    #region Execute

    /// <summary>Executes the plan over a materialized sequence and returns the page.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The sequence the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <returns>The paged result, or a validation error.</returns>
    Result<PagedResult<T>> Execute<T>(IEnumerable<T> source, QueryPlan<T> plan) where T : class;

    #endregion
}
