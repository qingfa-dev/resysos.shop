namespace BuildingBlock.Querying.Dapper;

/// <summary>
/// The Dapper provider executor. Dapper returns materialized rows, so the executor composes a
/// compiled <see cref="QueryPlan{T}"/> over the sequence a Dapper query produced: filter, ordering
/// per <see cref="QuerySort{T}"/>, then offset paging. <c>Execute</c> counts the filtered
/// set before sort and paging, then materializes the requested page. Failures stay on the railway;
/// composition never throws.
/// </summary>
/// <remarks>
/// The plan's filter and sorts are applied in memory over the rows Dapper returned. Use it for
/// reads where the base SQL already restricts the row set; for SQL-side filtering and paging keep
/// the EF provider.
/// </remarks>
public sealed class DapperQueryPlanExecutor : IDapperQueryPlanExecutor
{
    #region Construction

    private DapperQueryPlanExecutor()
    {
    }

    #endregion

    #region Factory

    /// <summary>Creates a Dapper plan executor.</summary>
    /// <returns>The executor.</returns>
    public static Result<DapperQueryPlanExecutor> Create()
        => Result.Success(new DapperQueryPlanExecutor());

    #endregion

    #region Build

    /// <inheritdoc />
    public Result<IQueryable<T>> Build<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
        => source is null
            ? Result.Failure<IQueryable<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : plan is null
                ? Result.Failure<IQueryable<T>>(
                    DapperQueryPlanExecutorConstant.Constraints.Plan.Required.Error)
                : Result.Success(BuildCore(source, plan));

    /// <inheritdoc />
    public Result<IEnumerable<T>> Build<T>(IEnumerable<T> source, QueryPlan<T> plan) where T : class
        => source is null
            ? Result.Failure<IEnumerable<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : Build(source.AsQueryable(), plan)
                .Map(static query => (IEnumerable<T>)query);

    #endregion

    #region Execute

    /// <inheritdoc />
    public Result<PagedResult<T>> Execute<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
    {
        if (source is null)
        {
            return Result.Failure<PagedResult<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error);
        }

        if (plan is null)
        {
            return Result.Failure<PagedResult<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Plan.Required.Error);
        }

        IQueryable<T> filtered = plan.Filter is null ? source : source.Where(plan.Filter);

        int totalCount = plan.IncludeTotalCount ? filtered.Count() : 0;

        IReadOnlyList<T> items = BuildCore(source, plan).ToList();

        return Result.Success(CreatePage(items, plan.Paging, totalCount));
    }

    /// <inheritdoc />
    public Result<PagedResult<T>> Execute<T>(IEnumerable<T> source, QueryPlan<T> plan) where T : class
        => source is null
            ? Result.Failure<PagedResult<T>>(
                DapperQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : Execute(source.AsQueryable(), plan);

    /// <inheritdoc />
    public ValueTask<Result<PagedResult<T>>> ExecuteAsync<T>(
        IQueryable<T> source,
        QueryPlan<T> plan,
        CancellationToken ct = default) where T : class
        => new(Execute(source, plan));

    #endregion

    #region Composition

    private static IQueryable<T> BuildCore<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
    {
        IQueryable<T> query = source;

        if (plan.Filter is not null)
        {
            query = query.Where(plan.Filter);
        }

        query = ApplySorts(query, plan.Sorts);

        if (plan.Paging is PagingPlan.Offset offset)
        {
            query = query.Skip(offset.Skip).Take(offset.Take);
        }

        return query;
    }

    private static IQueryable<T> ApplySorts<T>(
        IQueryable<T> query,
        IReadOnlyList<QuerySort<T>> sorts) where T : class
    {
        // A hand-built plan can carry a null collection or a null key selector; skip them so
        // composition never throws.
        if (sorts is null)
        {
            return query;
        }

        IOrderedQueryable<T>? ordered = null;

        foreach (QuerySort<T> sort in sorts)
        {
            if (sort is null || sort.KeySelector is null)
            {
                continue;
            }

            ordered = ordered is null
                ? Order(query, sort)
                : ThenOrder(ordered, sort);
        }

        return ordered ?? query;
    }

    private static IOrderedQueryable<T> Order<T>(IQueryable<T> source, QuerySort<T> sort) where T : class
        => sort.Descending
            ? source.OrderByDescending(sort.KeySelector)
            : source.OrderBy(sort.KeySelector);

    private static IOrderedQueryable<T> ThenOrder<T>(IOrderedQueryable<T> source, QuerySort<T> sort) where T : class
        => sort.Descending
            ? source.ThenByDescending(sort.KeySelector)
            : source.ThenBy(sort.KeySelector);

    private static PagedResult<T> CreatePage<T>(
        IReadOnlyList<T> items,
        PagingPlan paging,
        int totalCount)
    {
        int page = PagingConstant.Defaults.Page;
        int pageSize = PagingConstant.Defaults.PageSize;

        if (paging is PagingPlan.Offset offset)
        {
            pageSize = offset.Take;
            page = offset.Take > 0
                ? (offset.Skip / offset.Take) + PagingConstant.Defaults.Page
                : PagingConstant.Defaults.Page;
        }

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    #endregion
}
