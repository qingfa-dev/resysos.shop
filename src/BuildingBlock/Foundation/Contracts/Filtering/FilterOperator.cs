namespace BuildingBlock.Contracts.Filtering;

/// <summary>The comparison an incoming filter clause applies to a field.</summary>
public enum FilterOperator
{
    #region Comparison

    /// <summary>Equal to the supplied value.</summary>
    Eq,

    /// <summary>Not equal to the supplied value.</summary>
    Ne,

    /// <summary>Greater than the supplied value.</summary>
    Gt,

    /// <summary>Greater than or equal to the supplied value.</summary>
    Gte,

    /// <summary>Less than the supplied value.</summary>
    Lt,

    /// <summary>Less than or equal to the supplied value.</summary>
    Lte,

    /// <summary>Contains the supplied text.</summary>
    Contains,

    /// <summary>Starts with the supplied text.</summary>
    StartsWith,

    /// <summary>Ends with the supplied text.</summary>
    EndsWith,

    /// <summary>Equals any of the supplied values.</summary>
    In,

    #endregion

    #region Null Checks

    /// <summary>The field is null.</summary>
    IsNull,

    /// <summary>The field is not null.</summary>
    IsNotNull,

    #endregion

    #region Extended

    /// <summary>Equal to none of the supplied values.</summary>
    NotIn,

    /// <summary>Within the inclusive range of two supplied values.</summary>
    Between,

    #endregion

    #region Collection

    /// <summary>At least one element of a collection matches the nested filter.</summary>
    Any,

    /// <summary>Every element of a collection matches the nested filter.</summary>
    All

    #endregion
}
