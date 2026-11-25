namespace BuildingBlock.Querying.Plan;

/// <summary>Describes how a query result is paged.</summary>
public abstract record PagingPlan
{
    #region Construction

    private PagingPlan() { }

    /// <summary>No paging is applied.</summary>
    public static PagingPlan None { get; } = new NoPaging();

    #endregion

    #region Cases

    /// <summary>No paging is applied.</summary>
    public sealed record NoPaging : PagingPlan;

    /// <summary>Offset-based paging.</summary>
    /// <param name="Skip">The number of items to skip.</param>
    /// <param name="Take">The number of items to take.</param>
    public sealed record Offset(int Skip, int Take) : PagingPlan;

    #endregion
}
