namespace BuildingBlock.Querying.Policy;

/// <summary>
/// A per-entity query policy: the exposure mode, the enforced caps, and the authoritative
/// field registry the engine validates against. Its properties are settable so the
/// <see cref="QueryEngineDependencyInjectionExtensions"/> registration callback can configure
/// a policy after construction.
/// </summary>
public sealed class QueryPolicy
{
    #region State

    /// <summary>Gets or sets how fields are exposed when they are not explicitly configured.</summary>
    public QueryExposure Exposure { get; set; } = QueryPolicyConstant.Defaults.Exposure;

    /// <summary>Gets or sets the page size applied when a request does not specify one.</summary>
    public int DefaultPageSize { get; set; } = QueryPolicyConstant.Defaults.DefaultPageSize;

    /// <summary>Gets or sets the maximum page size the policy allows.</summary>
    public int MaxPageSize { get; set; } = QueryPolicyConstant.Defaults.MaxPageSize;

    /// <summary>Gets or sets the maximum filter nesting depth the policy allows.</summary>
    public int MaxFilterDepth { get; set; } = QueryPolicyConstant.Defaults.MaxFilterDepth;

    /// <summary>Gets or sets the maximum number of filter conditions the policy allows.</summary>
    public int MaxFilterConditions { get; set; } = QueryPolicyConstant.Defaults.MaxFilterConditions;

    /// <summary>Gets or sets the authoritative field registry for the policy.</summary>
    public QueryFieldRegistry Registry { get; set; } = new();

    #endregion
}
