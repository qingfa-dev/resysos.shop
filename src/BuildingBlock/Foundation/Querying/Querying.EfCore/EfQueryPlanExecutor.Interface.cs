namespace BuildingBlock.Querying.EfCore;

/// <summary>Applies a compiled query plan to an EF Core queryable source.</summary>
public interface IEfQueryPlanExecutor : IQueryPlanExecutor
{
    #region Execute

    /// <inheritdoc />
    new ValueTask<Result<PagedResult<T>>> ExecuteAsync<T>(
        IQueryable<T> source,
        QueryPlan<T> plan,
        CancellationToken ct = default) where T : class;

    #endregion
}
