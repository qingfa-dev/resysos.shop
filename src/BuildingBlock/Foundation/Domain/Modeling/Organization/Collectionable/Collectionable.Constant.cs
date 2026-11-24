namespace BuildingBlock.Domain.Modeling.Organization.Collectionable;

/// <summary>Unified constants and error definitions for the <see cref="Collectionable"/> concern.</summary>
public static partial class CollectionableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the collectionable entity.</summary>
    public static class Entity
    {
        /// <summary>Error for when the entity is required but null.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "collectionable.entity.required",
                message: "Collectionable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a collection value.</summary>
    public static class Collection
    {
        /// <summary>Error for when the collection is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.required",
                message: "Collection must be specified.");
        }

        /// <summary>Error for when the collection identifier is empty.</summary>
        public static class Empty
        {
            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.empty",
                message: "Collection identifier must not be empty.");
        }

        /// <summary>Maximum length for a string collection name.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.too_long",
                message: $"Collection name cannot exceed {Value} characters.");
        }

        /// <summary>Maximum number of collections per entity.</summary>
        public static class MaxCount
        {
            public const int Value = 32;

            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.limit_exceeded",
                message: $"Entity cannot have more than {Value} collections.");
        }

        /// <summary>Error for when the collection is already assigned.</summary>
        public static class Duplicate
        {
            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.duplicate",
                message: "Collection is already assigned.");
        }

        /// <summary>Error for when the collection is not assigned.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "collectionable.collection.not_found",
                message: "Collection is not assigned.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default option values for the <see cref="Collectionable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Default value for trimming string collections before validation.</summary>
        public const bool TrimValues = true;
    }

    #endregion
}
