namespace BuildingBlock.Querying.Facade;

/// <summary>
/// The public entry point of the dynamic query engine. It compiles a <see cref="QueryRequest"/>
/// into a validated <see cref="QueryPlan{T}"/> through <see cref="QueryCompiler"/> (which runs the
/// security gate) and then delegates execution to a provider <see cref="IQueryPlanExecutor"/>.
/// Every failure stays on the railway; the engine never throws.
/// </summary>
public class QueryEngine : IQueryEngine
{
    #region Construction

    private readonly QueryPolicy _policy;
    private readonly IQueryPlanExecutor _executor;

    /// <summary>Initializes a new instance of the <see cref="QueryEngine"/> class.</summary>
    /// <param name="policy">The policy supplying the field allowlist and caps.</param>
    /// <param name="executor">The provider executor the compiled plan is delegated to.</param>
    protected QueryEngine(QueryPolicy policy, IQueryPlanExecutor executor)
    {
        _policy = policy;
        _executor = executor;
    }

    #endregion

    #region Factory

    /// <summary>Creates a query engine over a policy and a provider executor.</summary>
    /// <param name="policy">The policy supplying the field allowlist and caps.</param>
    /// <param name="executor">The provider executor the compiled plan is delegated to.</param>
    /// <returns>The engine, or a validation error when an argument is missing.</returns>
    public static Result<QueryEngine> Create(QueryPolicy policy, IQueryPlanExecutor executor)
        => policy is null
            ? Result.Failure<QueryEngine>(QueryEngineConstant.Constraints.Policy.Required.Error)
            : executor is null
                ? Result.Failure<QueryEngine>(QueryEngineConstant.Constraints.Executor.Required.Error)
                : Result.Success(new QueryEngine(policy, executor));

    #endregion

    #region Page

    /// <inheritdoc />
    public Result<PagedResult<T>> Page<T>(
        IQueryable<T> source,
        QueryRequest request,
        QueryContext? context = null) where T : class
    {
        var plan = Compile<T>(request, context);

        return plan.IsFailure
            ? Result.Failure<PagedResult<T>>(plan.Errors)
            : _executor.Execute(source, plan.Value);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<T>>> PageAsync<T>(
        IQueryable<T> source,
        QueryRequest request,
        QueryContext? context = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var plan = Compile<T>(request, context);
        if (plan.IsFailure)
        {
            return Result.Failure<PagedResult<T>>(plan.Errors);
        }

        // Thread the context token and the caller token together; either may cancel execution.
        CancellationToken contextToken = context?.CancellationToken ?? CancellationToken.None;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(contextToken, cancellationToken);

        return await _executor.ExecuteAsync(source, plan.Value, linked.Token).ConfigureAwait(false);
    }

    #endregion

    #region Compile

    private Result<QueryPlan<T>> Compile<T>(QueryRequest? request, QueryContext? context)
        => QueryCompiler.Compile<T>(request, _policy, context ?? QueryContext.Anonymous);

    #endregion
}
