namespace BuildingBlock.Contracts.Filtering;

/// <summary>How the children of a <see cref="FilterGroup"/> combine.</summary>
public enum FilterGroupOperator
{
    #region Members

    /// <summary>Every child must match.</summary>
    And,

    /// <summary>At least one child must match.</summary>
    Or

    #endregion
}
