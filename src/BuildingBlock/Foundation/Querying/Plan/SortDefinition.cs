namespace BuildingBlock.Querying.Plan;

/// <summary>Describes a single ordering applied to a query result.</summary>
/// <param name="Field">The field path to order by.</param>
/// <param name="Descending">Whether the ordering is descending.</param>
public sealed record SortDefinition(string Field, bool Descending)
{
    #region State

    public string Field { get; init; } = Field;

    public bool Descending { get; init; } = Descending;

    #endregion
}
