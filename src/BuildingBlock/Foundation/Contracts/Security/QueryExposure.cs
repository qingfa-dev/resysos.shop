namespace BuildingBlock.Contracts.Security;

/// <summary>
/// Controls whether entity properties are exposed to the query engine by default.
/// The exposure mode is the base allowlist; the fluent field registry is the override.
/// </summary>
public enum QueryExposure
{
    #region Members

    /// <summary>Only explicitly allowed fields are exposed (opt-in; recommended).</summary>
    DenyByDefault = 0,

    /// <summary>Every property is exposed unless it is hidden (opt-out).</summary>
    AllowByDefault = 1

    #endregion
}
