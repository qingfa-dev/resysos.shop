namespace BuildingBlock.Querying.Policy;

/// <summary>
/// A resolved entry of the field registry: the external field name, the member accessor
/// that backs it, and its filter/sort/select/search grants.
/// </summary>
/// <param name="Name">The external field name used by clients.</param>
/// <param name="Accessor">The member access expression that backs the field.</param>
/// <param name="Filterable">The filter operators the field allows.</param>
/// <param name="Sortable">Whether the field may be used to order results.</param>
/// <param name="Selectable">Whether the field may be projected.</param>
/// <param name="Searchable">Whether the field participates in free-text search.</param>
public sealed record QueryField(
    string Name,
    LambdaExpression Accessor,
    FilterOperators Filterable,
    bool Sortable,
    bool Selectable,
    bool Searchable)
{
    #region State

    /// <summary>Gets the external field name used by clients.</summary>
    public string Name { get; init; } = Name;

    /// <summary>Gets the member access expression that backs the field.</summary>
    public LambdaExpression Accessor { get; init; } = Accessor;

    /// <summary>Gets the filter operators the field allows.</summary>
    public FilterOperators Filterable { get; init; } = Filterable;

    /// <summary>Gets a value indicating whether the field may be used to order results.</summary>
    public bool Sortable { get; init; } = Sortable;

    /// <summary>Gets a value indicating whether the field may be projected.</summary>
    public bool Selectable { get; init; } = Selectable;

    /// <summary>Gets a value indicating whether the field participates in free-text search.</summary>
    public bool Searchable { get; init; } = Searchable;

    #endregion
}
