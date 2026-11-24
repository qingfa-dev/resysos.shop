namespace BuildingBlock.Domain.Modeling.Content.Sluggable;

/// <summary>Unified constants and error definitions for the <see cref="Sluggable"/> concern.</summary>
public static partial class SluggableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the sluggable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the sluggable entity is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "sluggable.entity.required",
                message: "Sluggable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a slug.</summary>
    public static class Slug
    {
        /// <summary>Error when the slug is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "sluggable.slug.required",
                message: "Slug must be specified.");
        }

        /// <summary>Minimum length for a slug.</summary>
        public static class MinLength
        {
            public const int Value = 1;

            public static readonly Error Error = Error.Validation(
                code: "sluggable.slug.too_short",
                message: $"Slug must be at least {Value} character(s).");
        }

        /// <summary>Maximum length for a slug.</summary>
        public static class MaxLength
        {
            public const int Value = 200;

            public static readonly Error Error = Error.Validation(
                code: "sluggable.slug.too_long",
                message: $"Slug cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a slug.</summary>
        public static class Format
        {
            public const string Pattern = @"^[a-z0-9]+(?:-[a-z0-9]+)*$";

            public static readonly Error Error = Error.Validation(
                code: "sluggable.slug.invalid",
                message: "Slug must contain only lowercase letters, digits and single hyphens.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the <see cref="Sluggable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Suggested fallback slug when a caller needs one for empty input.</summary>
        public const string FallbackSlug = "n-a";
    }

    #endregion
}
