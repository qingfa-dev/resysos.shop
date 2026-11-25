namespace BuildingBlock.Contracts.Security;

#region Declaration

/// <summary>
/// Declares that a property is never exposed to the query engine, regardless of the
/// configured exposure mode.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class HiddenAttribute : Attribute
{
}

#endregion
