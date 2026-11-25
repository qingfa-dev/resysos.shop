namespace BuildingBlock.Querying.Plan;

/// <summary>An immutable, validated description of a query.</summary>
public sealed record QueryPlan<T>
{
    #region State

    public Expression<Func<T, bool>>? Filter { get; init; }

    public IReadOnlyList<QuerySort<T>> Sorts { get; init; } = [];

    public PagingPlan Paging { get; init; } = PagingPlan.None;

    public bool IncludeTotalCount { get; init; }

    #endregion
}
