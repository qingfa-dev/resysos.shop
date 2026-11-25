namespace BuildingBlock.Querying.Execution;

/// <summary>
/// The public entry point of the dynamic query engine. It compiles a <see cref="QueryRequest"/>
/// into a validated <see cref="QueryPlan{T}"/> and executes it through a provider executor.
/// </summary>
public interface IQueryEngine
{
    #region Page

    /// <summary>Compiles a request and materializes the requested page.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    Result<PagedResult<T>> Page<T>(
        IQueryable<T> source,
        QueryRequest request,
        QueryContext? context = null) where T : class;

    /// <summary>Compiles a request and materializes the requested page asynchronously.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <param name="cancellationToken">An optional token combined with the context token.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    Task<Result<PagedResult<T>>> PageAsync<T>(
        IQueryable<T> source,
        QueryRequest request,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class;

    #endregion
}
