namespace BuildingBlock.Domain.Modeling.Foundation.Identifiable;

/// <summary>Unified constants and error definitions for the Identifiable concern.</summary>
public static partial class IdentifiableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the identifiable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity itself is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "identifiable.entity.required",
                message: "Identifiable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the identifier.</summary>
    public static class Id
    {
        /// <summary>Error when the identifier is missing or default.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "identifiable.id.required",
                message: "Identifier must be specified (default values are not allowed).");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the Identifiable concern.</summary>
    public static class Defaults
    {
        /// <summary>Empty identifier sentinel for <see cref="Guid"/> keys.</summary>
        public static readonly Guid EmptyId = Guid.Empty;
    }

    #endregion
}
