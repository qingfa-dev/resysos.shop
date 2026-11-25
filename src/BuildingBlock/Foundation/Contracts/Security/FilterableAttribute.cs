using BuildingBlock.Contracts.Filtering;

namespace BuildingBlock.Contracts.Security;

#region Declaration

/// <summary>
/// Declares which filter operators a property allows. The allowlist is the base grant;
/// the fluent registry overrides it.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FilterableAttribute : Attribute
{
    /// <summary>Gets the filter operators the property allows.</summary>
    public FilterOperators Operators { get; init; } = FilterOperators.Eq;
}

#endregion
