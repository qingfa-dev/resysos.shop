namespace BuildingBlock.Specifications.EfCore;

/// <summary>EF Core specification evaluator settings.</summary>
public sealed class EfSpecificationOptions
{
    #region Constants

    /// <summary>The configuration section for the provider.</summary>
    public const string SectionName = "BuildingBlocks:Domain:Specifications:EfCore";

    #endregion

    #region Properties

    /// <summary>Gets or sets whether collection includes use split queries.</summary>
    public bool UseSplitQuery { get; set; } = EfSpecificationConstant.Defaults.UseSplitQuery;

    /// <summary>Gets or sets whether global query filters are ignored.</summary>
    public bool IgnoreQueryFilters { get; set; } =
        EfSpecificationConstant.Defaults.IgnoreQueryFilters;

    #endregion
}
