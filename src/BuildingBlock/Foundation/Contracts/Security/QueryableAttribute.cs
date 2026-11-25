namespace BuildingBlock.Contracts.Security;

#region Declaration

/// <summary>
/// Declares that a property is exposed to the query engine and selectable by default.
/// Attributes are role-agnostic and form the base allowlist; the fluent registry overrides them.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class QueryableAttribute : Attribute
{
}

#endregion
