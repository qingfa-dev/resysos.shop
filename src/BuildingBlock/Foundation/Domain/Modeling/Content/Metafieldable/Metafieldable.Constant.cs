namespace BuildingBlock.Domain.Modeling.Content.Metafieldable;

/// <summary>Unified constants and error definitions for the <see cref="Metafieldable"/> concern.</summary>
public static partial class MetafieldableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the metafieldable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the metafieldable entity is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "metafieldable.entity.required",
                message: "Metafieldable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a metafield namespace.</summary>
    public static class Namespace
    {
        /// <summary>Error when the namespace is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "metafieldable.namespace.required",
                message: "Metafield namespace must be specified.");
        }

        /// <summary>Minimum length for a namespace.</summary>
        public static class MinLength
        {
            public const int Value = 1;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.namespace.too_short",
                message: $"Namespace must be at least {Value} character(s).");
        }

        /// <summary>Maximum length for a namespace.</summary>
        public static class MaxLength
        {
            public const int Value = 64;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.namespace.too_long",
                message: $"Namespace cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a namespace.</summary>
        public static class Format
        {
            public const string Pattern = @"^[a-z0-9_]+$";

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.namespace.invalid",
                message: "Namespace must be snake_case (lowercase letters, digits, underscores).");
        }
    }

    /// <summary>Constraints and error definitions for a metafield key.</summary>
    public static class Key
    {
        /// <summary>Error when the key is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "metafieldable.key.required",
                message: "Metafield key must be specified.");
        }

        /// <summary>Minimum length for a key.</summary>
        public static class MinLength
        {
            public const int Value = 1;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.key.too_short",
                message: $"Key must be at least {Value} character(s).");
        }

        /// <summary>Maximum length for a key.</summary>
        public static class MaxLength
        {
            public const int Value = 64;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.key.too_long",
                message: $"Key cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a key.</summary>
        public static class Format
        {
            public const string Pattern = @"^[a-z0-9_]+$";

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.key.invalid",
                message: "Key must be snake_case (lowercase letters, digits, underscores).");
        }
    }

    /// <summary>Constraints and error definitions for a metafield value.</summary>
    public static class Value
    {
        /// <summary>Maximum length for a metafield value.</summary>
        public static class MaxLength
        {
            public const int Value = 8000;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.value.too_long",
                message: $"Metafield value cannot exceed {Value} characters.");
        }
    }

    /// <summary>Constraints and error definitions for the metafield collection.</summary>
    public static class Collection
    {
        /// <summary>Maximum number of metafields allowed on an entity.</summary>
        public static class MaxMetafields
        {
            public const int Value = 100;

            public static readonly Error Error = Error.Validation(
                code: "metafieldable.metafield.too_many",
                message: $"Cannot store more than {Value} metafields.");
        }
    }

    /// <summary>Error when the metafield is not present.</summary>
    public static class Metafield
    {
        /// <summary>Error when the (namespace, key) pair is not present.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "metafieldable.metafield.not_found",
                message: "Metafield is not present.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the <see cref="Metafieldable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Defaults for the metafield type discriminator.</summary>
        public static class Type
        {
            /// <summary>Default metafield type when none is specified.</summary>
            public const MetafieldType Default = MetafieldType.ShortText;
        }
    }

    #endregion
}
