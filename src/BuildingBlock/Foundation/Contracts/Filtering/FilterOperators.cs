namespace BuildingBlock.Contracts.Filtering;

/// <summary>
/// A bit mask over <see cref="FilterOperator"/> describing which operators a field allows.
/// It is a separate type so <see cref="FilterOperator"/> keeps its stable, sequential numbering.
/// </summary>
[Flags]
public enum FilterOperators
{
    #region Members

    /// <summary>No operator is allowed.</summary>
    None = 0,

    /// <summary>Allows <see cref="FilterOperator.Eq"/>.</summary>
    Eq = 1 << 0,

    /// <summary>Allows <see cref="FilterOperator.Ne"/>.</summary>
    Ne = 1 << 1,

    /// <summary>Allows <see cref="FilterOperator.Gt"/>.</summary>
    Gt = 1 << 2,

    /// <summary>Allows <see cref="FilterOperator.Gte"/>.</summary>
    Gte = 1 << 3,

    /// <summary>Allows <see cref="FilterOperator.Lt"/>.</summary>
    Lt = 1 << 4,

    /// <summary>Allows <see cref="FilterOperator.Lte"/>.</summary>
    Lte = 1 << 5,

    /// <summary>Allows <see cref="FilterOperator.Contains"/>.</summary>
    Contains = 1 << 6,

    /// <summary>Allows <see cref="FilterOperator.StartsWith"/>.</summary>
    StartsWith = 1 << 7,

    /// <summary>Allows <see cref="FilterOperator.EndsWith"/>.</summary>
    EndsWith = 1 << 8,

    /// <summary>Allows <see cref="FilterOperator.In"/>.</summary>
    In = 1 << 9,

    /// <summary>Allows <see cref="FilterOperator.IsNull"/>.</summary>
    IsNull = 1 << 10,

    /// <summary>Allows <see cref="FilterOperator.IsNotNull"/>.</summary>
    IsNotNull = 1 << 11,

    /// <summary>Allows <see cref="FilterOperator.NotIn"/>.</summary>
    NotIn = 1 << 12,

    /// <summary>Allows <see cref="FilterOperator.Between"/>.</summary>
    Between = 1 << 13,

    /// <summary>Allows <see cref="FilterOperator.Any"/>.</summary>
    Any = 1 << 14,

    /// <summary>Allows <see cref="FilterOperator.All"/> (collection all).</summary>
    CollectionAll = 1 << 15,

    /// <summary>Allows every operator.</summary>
    All = Eq | Ne | Gt | Gte | Lt | Lte | Contains | StartsWith | EndsWith | In
        | IsNull | IsNotNull | NotIn | Between | Any | CollectionAll

    #endregion
}
