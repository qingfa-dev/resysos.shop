namespace BuildingBlock.Querying.EfCore;

/// <summary>
/// The EF Core provider executor. It composes a compiled <see cref="QueryPlan{T}"/> onto an
/// <see cref="IQueryable{T}"/> so the filter, ordering and paging translate to SQL: no-tracking,
/// then <c>Where</c>, then ordering per <see cref="QuerySort{T}"/>, then offset paging.
/// <see cref="ExecuteAsync{T}"/> counts the filtered query before sort and paging, then
/// materializes the requested page. Failures stay on the railway; composition never throws.
/// </summary>
public sealed class EfQueryPlanExecutor : IEfQueryPlanExecutor
{
    #region Construction

    private EfQueryPlanExecutor()
    {
    }

    #endregion

    #region Factory

    /// <summary>Creates an EF Core plan executor.</summary>
    /// <returns>The executor.</returns>
    public static Result<EfQueryPlanExecutor> Create()
        => Result.Success(new EfQueryPlanExecutor());

    #endregion

    #region Build

    /// <inheritdoc />
    public Result<IQueryable<T>> Build<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
        => source is null
            ? Result.Failure<IQueryable<T>>(
                EfQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : plan is null
                ? Result.Failure<IQueryable<T>>(
                    EfQueryPlanExecutorConstant.Constraints.Plan.Required.Error)
                : Result.Success(BuildCore(source, plan));

    #endregion

    #region Execute

    /// <inheritdoc />
    public Result<PagedResult<T>> Execute<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
    {
        if (source is null)
        {
            return Result.Failure<PagedResult<T>>(
                EfQueryPlanExecutorConstant.Constraints.Source.Required.Error);
        }

        if (plan is null)
        {
            return Result.Failure<PagedResult<T>>(
                EfQueryPlanExecutorConstant.Constraints.Plan.Required.Error);
        }

        IQueryable<T> filtered = plan.Filter is null ? source : source.Where(plan.Filter);

        int totalCount = plan.IncludeTotalCount ? filtered.Count() : 0;

        IReadOnlyList<T> items = BuildCore(source, plan).ToList();

        return Result.Success(CreatePage(items, plan.Paging, totalCount));
    }

    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<T>>> ExecuteAsync<T>(
        IQueryable<T> source,
        QueryPlan<T> plan,
        CancellationToken ct = default) where T : class
    {
        if (source is null)
        {
            return Result.Failure<PagedResult<T>>(
                EfQueryPlanExecutorConstant.Constraints.Source.Required.Error);
        }

        if (plan is null)
        {
            return Result.Failure<PagedResult<T>>(
                EfQueryPlanExecutorConstant.Constraints.Plan.Required.Error);
        }

        IQueryable<T> filtered = plan.Filter is null ? source : source.Where(plan.Filter);

        int totalCount = plan.IncludeTotalCount
            ? await filtered.CountAsync(ct).ConfigureAwait(false)
            : 0;

        IReadOnlyList<T> items = await BuildCore(source, plan)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result.Success(CreatePage(items, plan.Paging, totalCount));
    }

    #endregion

    #region Composition

    private static IQueryable<T> BuildCore<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
    {
        IQueryable<T> query = source.AsNoTracking();

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
