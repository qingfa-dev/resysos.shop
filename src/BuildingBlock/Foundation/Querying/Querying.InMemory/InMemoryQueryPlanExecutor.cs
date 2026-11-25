namespace BuildingBlock.Querying.InMemory;

/// <summary>
/// The in-memory provider executor. It applies a compiled <see cref="QueryPlan{T}"/> to an
/// <see cref="IEnumerable{T}"/>: the filter first, then ordering per <see cref="QuerySort{T}"/>,
/// then offset paging. Failures stay on the railway; execution never throws.
/// </summary>
public sealed class InMemoryQueryPlanExecutor : IInMemoryQueryPlanExecutor
{
    #region Construction

    private InMemoryQueryPlanExecutor()
    {
    }

    #endregion

    #region Factory

    /// <summary>Creates an in-memory plan executor.</summary>
    /// <returns>The executor.</returns>
    public static Result<InMemoryQueryPlanExecutor> Create()
        => Result.Success(new InMemoryQueryPlanExecutor());

    #endregion

    #region Build

    /// <inheritdoc />
    public Result<IEnumerable<T>> Build<T>(IEnumerable<T> source, QueryPlan<T> plan)
        => source is null
            ? Result.Failure<IEnumerable<T>>(
                InMemoryQueryPlanExecutorConstant.Constraints.Source.Required.Error)
            : plan is null
                ? Result.Failure<IEnumerable<T>>(
                    InMemoryQueryPlanExecutorConstant.Constraints.Plan.Required.Error)
                : Result.Success(BuildCore(source, plan));

    #endregion

    #region Composition

    private static IEnumerable<T> BuildCore<T>(IEnumerable<T> source, QueryPlan<T> plan)
    {
        IEnumerable<T> result = source;

        if (plan.Filter is not null)
        {
            result = result.Where(plan.Filter.Compile());
        }

        IOrderedEnumerable<T>? ordered = null;

        // A hand-built plan can carry a null collection or a null key selector; skip them so
        // execution never throws.
        if (plan.Sorts is not null)
        {
            foreach (QuerySort<T> sort in plan.Sorts)
            {
                if (sort is null || sort.KeySelector is null)
                {
                    continue;
                }

                Func<T, object> key = sort.KeySelector.Compile();
                ordered = ordered is null
                    ? Order(result, key, sort.Descending)
                    : ThenOrder(ordered, key, sort.Descending);
            }
        }

        if (ordered is not null)
        {
            result = ordered;
        }

        if (plan.Paging is PagingPlan.Offset offset)
        {
            result = result.Skip(offset.Skip).Take(offset.Take);
        }

        return result;
    }

    private static IOrderedEnumerable<T> Order<T>(
        IEnumerable<T> source,
        Func<T, object> key,
        bool descending)
        => descending ? source.OrderByDescending(key) : source.OrderBy(key);

    private static IOrderedEnumerable<T> ThenOrder<T>(
        IOrderedEnumerable<T> source,
        Func<T, object> key,
        bool descending)
        => descending ? source.ThenByDescending(key) : source.ThenBy(key);

    #endregion
}
