namespace BuildingBlock.Querying.Dapper;

/// <summary>
/// Extensions that run a <see cref="QueryRequest"/> over a Dapper-materialized sequence through a
/// <see cref="QueryEngine"/>, and materialize a page.
/// </summary>
public static class DapperQueryEngineExtensions
{
    #region ApplyDynamicQuery

    /// <summary>Applies a request to a materialized sequence through a supplied engine.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The sequence the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Result<PagedResult<T>> ApplyDynamicQuery<T>(
        this IEnumerable<T> source,
        QueryRequest request,
        QueryEngine engine,
        QueryContext? context = null) where T : class
        => engine.Page(source.AsQueryable(), request, context);

    /// <summary>Applies a request to a materialized sequence asynchronously.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The sequence the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <param name="cancellationToken">An optional token combined with the context token.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Task<Result<PagedResult<T>>> ApplyDynamicQueryAsync<T>(
        this IEnumerable<T> source,
        QueryRequest request,
        QueryEngine engine,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class
        => engine.PageAsync(source.AsQueryable(), request, context, cancellationToken);

    #endregion
}
