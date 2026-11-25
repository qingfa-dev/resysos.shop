namespace BuildingBlock.Querying.Plan;

/// <summary>Describes one resolved ordering applied to a query result.</summary>
/// <param name="KeySelector">The resolved key selector the result is ordered by.</param>
/// <param name="Descending">Whether the ordering is descending.</param>
public sealed record QuerySort<T>(Expression<Func<T, object>> KeySelector, bool Descending)
{
    #region State

    /// <summary>Gets the resolved key selector the result is ordered by.</summary>
    public Expression<Func<T, object>> KeySelector { get; init; } = KeySelector;

    /// <summary>Gets a value indicating whether the ordering is descending.</summary>
    public bool Descending { get; init; } = Descending;

    #endregion
}
