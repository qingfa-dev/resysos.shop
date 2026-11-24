namespace BuildingBlock.Contracts.Sorting;

/// <summary>One sort clause as the client sent it.</summary>
public sealed record SortDescriptor
{
    #region State

    /// <summary>Gets the requested field name.</summary>
    public required string Field { get; init; }

    /// <summary>Gets the sort direction.</summary>
    public SortDirection Direction { get; init; } = SortConstant.Defaults.Direction;

    #endregion
}
