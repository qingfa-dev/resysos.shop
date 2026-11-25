namespace BuildingBlock.Contracts.Filtering;

#region Base

/// <summary>Base type for the nodes of a filter expression tree.</summary>
public abstract record FilterNode;

#endregion

#region Conditions

/// <summary>
/// A leaf filter clause: one field compared to a value (or values) with one operator.
/// </summary>
public sealed record FilterCondition : FilterNode
{
    #region State

    /// <summary>Gets the field name the clause targets.</summary>
    public required string Field { get; init; }

    /// <summary>Gets the comparison to apply.</summary>
    public FilterOperator Operator { get; init; } = FilterConstant.Defaults.Operator;

    /// <summary>Gets the raw value for the single-value operators.</summary>
    public string? Value { get; init; }

    /// <summary>Gets the raw values for the multi-value operators.</summary>
    public IReadOnlyList<string> Values { get; init; } = [];

    /// <summary>Gets a value indicating whether text comparisons ignore case.</summary>
    public bool CaseInsensitive { get; init; }

    #endregion
}

#endregion

#region Groups

/// <summary>A group of child nodes combined by a boolean operator.</summary>
public sealed record FilterGroup : FilterNode
{
    #region State

    /// <summary>Gets how the children combine.</summary>
    public FilterGroupOperator Operator { get; init; } = FilterGroupOperator.And;

    /// <summary>Gets the child nodes.</summary>
    public IReadOnlyList<FilterNode> Children { get; init; } = [];

    #endregion
}

#endregion

#region Negation

/// <summary>The logical negation of a child node.</summary>
public sealed record FilterNegation : FilterNode
{
    #region State

    /// <summary>Gets the negated child.</summary>
    public required FilterNode Child { get; init; }

    #endregion
}

#endregion
