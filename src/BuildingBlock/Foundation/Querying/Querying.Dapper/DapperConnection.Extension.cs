namespace BuildingBlock.Querying.Dapper;

/// <summary>
/// Runs a Dapper query and applies a <see cref="QueryRequest"/> to the rows it returns. The base
/// SQL runs on the database; the plan's filter, ordering and paging are applied to the materialized
/// rows by the <see cref="QueryEngine"/>.
/// </summary>
public static class DapperConnectionExtensions
{
    #region QueryDynamic

    /// <summary>Runs a Dapper query and applies a request to its rows.</summary>
    /// <typeparam name="T">The row type Dapper materializes.</typeparam>
    /// <param name="connection">The open database connection.</param>
    /// <param name="sql">The base SQL the rows are read with.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="parameters">Optional Dapper parameters for the base SQL.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static Result<PagedResult<T>> QueryDynamic<T>(
        this IDbConnection connection,
        string sql,
        QueryRequest request,
        QueryEngine engine,
        object? parameters = null,
        QueryContext? context = null) where T : class
        => connection is null
            ? Result.Failure<PagedResult<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : connection.Query<T>(sql, parameters).ApplyDynamicQuery(request, engine, context);

    /// <summary>Runs a Dapper query asynchronously and applies a request to its rows.</summary>
    /// <typeparam name="T">The row type Dapper materializes.</typeparam>
    /// <param name="connection">The open database connection.</param>
    /// <param name="sql">The base SQL the rows are read with.</param>
    /// <param name="request">The request to compile and execute.</param>
    /// <param name="engine">The engine that compiles and executes the request.</param>
    /// <param name="parameters">Optional Dapper parameters for the base SQL.</param>
    /// <param name="context">The caller context, or <see langword="null"/> for anonymous access.</param>
    /// <param name="cancellationToken">An optional token combined with the context token.</param>
    /// <returns>The paged result, or the first validation error encountered.</returns>
    public static async Task<Result<PagedResult<T>>> QueryDynamicAsync<T>(
        this IDbConnection connection,
        string sql,
        QueryRequest request,
        QueryEngine engine,
        object? parameters = null,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (connection is null)
        {
            return Result.Failure<PagedResult<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error);
        }

        IEnumerable<T> rows = await connection
            .QueryAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return rows.ApplyDynamicQuery(request, engine, context);
    }

    #endregion
}
