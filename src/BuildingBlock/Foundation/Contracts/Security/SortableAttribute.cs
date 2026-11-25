namespace BuildingBlock.Contracts.Security;

#region Declaration

/// <summary>Declares that a property may be used to order query results.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SortableAttribute : Attribute
{
}

#endregion
