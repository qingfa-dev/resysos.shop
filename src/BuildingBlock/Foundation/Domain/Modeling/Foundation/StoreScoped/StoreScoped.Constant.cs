namespace BuildingBlock.Domain.Modeling.Foundation.StoreScoped;

/// <summary>Unified constants and error definitions for the StoreScoped concern.</summary>
public static partial class StoreScopedConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the store-scoped entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity itself is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "store_scoped.entity.required",
                message: "Store-scoped entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the store identifier.</summary>
    public static class Store
    {
        /// <summary>Error when the store identifier is the default value.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "store_scoped.store_id.required",
                message: "Store identifier must be specified (default values are not allowed).");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the StoreScoped concern.</summary>
    public static class Defaults
    {
        /// <summary>Empty store identifier sentinel for <see cref="Guid"/> keys.</summary>
        public static readonly Guid EmptyStoreId = Guid.Empty;
    }

    #endregion
}
