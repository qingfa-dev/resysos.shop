using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Querying.Facade;

/// <summary>
/// Extensions that run a <see cref="QueryRequest"/> through a <see cref="QueryEngine"/> supplied
/// directly or resolved from an <see cref="IServiceProvider"/>, and materialize a page.
/// </summary>
public static class QueryEngineExtensions
{
    #region ApplyDynamicQuery

    /// <summary>Applies a request through a supplied engine.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Result<PagedResult<T>> ApplyDynamicQuery<T>(
        this IQueryable<T> source,
        QueryRequest request,
        QueryEngine engine,
        QueryContext? context = null) where T : class
        => engine.Page(source, request, context);

    /// <summary>Applies a request through the engine registered with the supplied services.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="services">The services the engine is resolved from.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Result<PagedResult<T>> ApplyDynamicQuery<T>(
        this IQueryable<T> source,
        QueryRequest request,
        IServiceProvider services,
        QueryContext? context = null) where T : class
    {
        QueryEngine? engine = Resolve(services);

        return engine is null
            ? Result.Failure<PagedResult<T>>(QueryEngineConstant.Constraints.Engine.NotRegistered.Error)
            : engine.Page(source, request, context);
    }

    #endregion

    #region ApplyDynamicQueryAsync

    /// <summary>Applies a request asynchronously through a supplied engine.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <param name="cancellationToken">An optional token combined with the context token.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Task<Result<PagedResult<T>>> ApplyDynamicQueryAsync<T>(
        this IQueryable<T> source,
        QueryRequest request,
        QueryEngine engine,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class
        => engine.PageAsync(source, request, context, cancellationToken);

    /// <summary>Applies a request asynchronously through the engine registered with the services.</summary>
    /// <typeparam name="T">The entity type the query targets.</typeparam>
    /// <param name="source">The queryable source the request is applied to.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="services">The services the engine is resolved from.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <param name="cancellationToken">An optional token combined with the context token.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Task<Result<PagedResult<T>>> ApplyDynamicQueryAsync<T>(
        this IQueryable<T> source,
        QueryRequest request,
        IServiceProvider services,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class
    {
        QueryEngine? engine = Resolve(services);

        return engine is null
            ? Task.FromResult(
                Result.Failure<PagedResult<T>>(QueryEngineConstant.Constraints.Engine.NotRegistered.Error))
            : engine.PageAsync(source, request, context, cancellationToken);
    }

    #endregion

    #region Resolution

    private static QueryEngine? Resolve(IServiceProvider? services)
        => services?.GetService<QueryEngine>();

    #endregion
}
