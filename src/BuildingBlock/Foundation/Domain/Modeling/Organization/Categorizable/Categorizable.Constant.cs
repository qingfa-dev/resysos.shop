namespace BuildingBlock.Domain.Modeling.Organization.Categorizable;

/// <summary>Unified constants and error definitions for the <see cref="Categorizable"/> concern.</summary>
public static partial class CategorizableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the categorizable entity.</summary>
    public static class Entity
    {
        /// <summary>Error for when the entity is required but null.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "categorizable.entity.required",
                message: "Categorizable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a category value.</summary>
    public static class Category
    {
        /// <summary>Error for when the category is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.required",
                message: "Category must be specified.");
        }

        /// <summary>Error for when the category identifier is empty.</summary>
        public static class Empty
        {
            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.empty",
                message: "Category identifier must not be empty.");
        }

        /// <summary>Maximum length for a string category name.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.too_long",
                message: $"Category name cannot exceed {Value} characters.");
        }

        /// <summary>Maximum number of categories per entity.</summary>
        public static class MaxCount
        {
            public const int Value = 32;

            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.limit_exceeded",
                message: $"Entity cannot have more than {Value} categories.");
        }

        /// <summary>Error for when the category is already assigned.</summary>
        public static class Duplicate
        {
            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.duplicate",
                message: "Category is already assigned.");
        }

        /// <summary>Error for when the category is not assigned.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "categorizable.category.not_found",
                message: "Category is not assigned.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default option values for the <see cref="Categorizable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Default value for trimming string categories before validation.</summary>
        public const bool TrimValues = true;
    }

    #endregion
}
