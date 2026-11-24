namespace BuildingBlock.Contracts.Filtering;

/// <summary>
/// One filter clause as the client sent it, with the value still untyped. Turning it into a
/// predicate is the job of the query specification provider.
/// </summary>
public sealed record FilterDescriptor
{
    #region State

    /// <summary>Gets the requested field name.</summary>
    public required string Field { get; init; }

    /// <summary>Gets the comparison to apply.</summary>
    public FilterOperator Operator { get; init; } = FilterConstant.Defaults.Operator;

    /// <summary>Gets the raw value for the single-value operators.</summary>
    public string? Value { get; init; }

    /// <summary>Gets the raw values for <see cref="FilterOperator.In"/>.</summary>
    public IReadOnlyList<string> Values { get; init; } = [];

    #endregion
}
