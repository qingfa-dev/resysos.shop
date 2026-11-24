namespace BuildingBlock.Domain.Modeling.Content.Taggable;

/// <summary>Unified constants and error definitions for the <see cref="Taggable"/> concern.</summary>
public static partial class TaggableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the taggable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the taggable entity is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "taggable.entity.required",
                message: "Taggable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a single tag.</summary>
    public static class Tag
    {
        /// <summary>Error when the tag is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.required",
                message: "Tag must be specified.");
        }

        /// <summary>Minimum length for a tag.</summary>
        public static class MinLength
        {
            public const int Value = 1;

            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.too_short",
                message: $"Tag must be at least {Value} character(s).");
        }

        /// <summary>Maximum length for a tag.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.too_long",
                message: $"Tag cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a tag.</summary>
        public static class Format
        {
            public const string Pattern = @"^\S(?:.*\S)?$";

            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.invalid",
                message: "Tag must not have leading or trailing whitespace.");
        }

        /// <summary>Error when the tag is already present.</summary>
        public static class Duplicate
        {
            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.duplicate",
                message: "Tag is already present.");
        }

        /// <summary>Error when the tag is not present.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "taggable.tag.not_found",
                message: "Tag is not present.");
        }
    }

    /// <summary>Constraints and error definitions for the tag collection.</summary>
    public static class Collection
    {
        /// <summary>Maximum number of tags allowed on an entity.</summary>
        public static class MaxTags
        {
            public const int Value = 50;

            public static readonly Error Error = Error.Validation(
                code: "taggable.tags.too_many",
                message: $"Cannot store more than {Value} tags.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the <see cref="Taggable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Normalization defaults applied to tags before validation.</summary>
        public static class Normalization
        {
            /// <summary>Whether surrounding whitespace is trimmed.</summary>
            public const bool TrimWhitespace = true;

            /// <summary>Whether tags are lowercased.</summary>
            public const bool ToLowercase = true;
        }
    }

    #endregion
}
