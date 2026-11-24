namespace BuildingBlock.Domain.Modeling.Content.MediaAttachable;

/// <summary>Unified constants and error definitions for the <see cref="MediaAttachable"/> concern.</summary>
public static partial class MediaAttachableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the media-attachable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the media-attachable entity is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.entity.required",
                message: "Media-attachable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the media itself.</summary>
    public static class Media
    {
        /// <summary>Error when the media is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media.required",
                message: "Media must be specified.");
        }

        /// <summary>Error when the media is already attached.</summary>
        public static class Duplicate
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media.duplicate",
                message: "Media is already attached.");
        }

        /// <summary>Error when the media is not attached.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media.not_found",
                message: "Media is not attached.");
        }
    }

    /// <summary>Constraints and error definitions for a media URL.</summary>
    public static class Url
    {
        /// <summary>Error when the media URL is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.url.required",
                message: "Media URL must be specified.");
        }

        /// <summary>Maximum length for a media URL.</summary>
        public static class MaxLength
        {
            public const int Value = 2048;

            public static readonly Error Error = Error.Validation(
                code: "media_attachable.url.too_long",
                message: $"Media URL cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a media URL.</summary>
        public static class Format
        {
            public const string Pattern = @"^https?://[^\s/$.?#].[^\s]*$";

            public static readonly Error Error = Error.Validation(
                code: "media_attachable.url.invalid",
                message: "Media URL must be a valid http(s) URL.");
        }
    }

    /// <summary>Constraints and error definitions for media alt text.</summary>
    public static class AltText
    {
        /// <summary>Maximum length for media alt text.</summary>
        public static class MaxLength
        {
            public const int Value = 500;

            public static readonly Error Error = Error.Validation(
                code: "media_attachable.alt_text.too_long",
                message: $"Media alt text cannot exceed {Value} characters.");
        }
    }

    /// <summary>Constraints and error definitions for the media type.</summary>
    public static class MediaType
    {
        /// <summary>Error when the media type is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media_type.required",
                message: "Media type must be specified.");
        }

        /// <summary>Allowed media type values.</summary>
        public static class Allowed
        {
            public static readonly IReadOnlyList<string> Values =
            [
                "image",
                "video",
                "audio",
                "document",
            ];

            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media_type.invalid",
                message: $"Media type must be one of: {string.Join(", ", Values)}.");
        }
    }

    /// <summary>Constraints and error definitions for the attached-media collection.</summary>
    public static class Collection
    {
        /// <summary>Maximum number of media items allowed on an entity.</summary>
        public static class MaxAttachments
        {
            public const int Value = 20;

            public static readonly Error Error = Error.Validation(
                code: "media_attachable.media.too_many",
                message: $"Cannot attach more than {Value} media items.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the <see cref="MediaAttachable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Default media type discriminator.</summary>
        public const string MediaType = "image";
    }

    #endregion
}
