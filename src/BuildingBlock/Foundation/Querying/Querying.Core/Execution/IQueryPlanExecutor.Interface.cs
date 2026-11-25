namespace BuildingBlock.Querying.Execution;

/// <summary>
/// The provider-execution seam. A provider implements it to compose an immutable
/// <see cref="QueryPlan{T}"/> onto its own queryable source. The plan is already compiled and
/// validated, so the executor only shapes the query; failures stay on the railway.
/// </summary>
public interface IQueryPlanExecutor
{
    #region Build

    /// <summary>Builds the executable query for a plan.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The provider source the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <returns>The composed query, or a validation error.</returns>
    Result<IQueryable<T>> Build<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class;

    #endregion

    #region Execute

    /// <summary>Executes the plan and materializes the requested page.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The provider source the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <returns>The paged result, or a validation error.</returns>
    Result<PagedResult<T>> Execute<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class;

    /// <summary>Executes the plan and materializes the requested page.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The provider source the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <param name="cancellationToken">The token used to cancel execution.</param>
    /// <returns>The paged result, or a validation error.</returns>
    ValueTask<Result<PagedResult<T>>> ExecuteAsync<T>(
        IQueryable<T> source,
        QueryPlan<T> plan,
        CancellationToken cancellationToken = default) where T : class;

    #endregion
}
