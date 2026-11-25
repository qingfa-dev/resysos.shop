namespace BuildingBlock.Contracts.Security;

#region Declaration

/// <summary>Declares that a property participates in the free-text search term.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SearchableAttribute : Attribute
{
}

#endregion
