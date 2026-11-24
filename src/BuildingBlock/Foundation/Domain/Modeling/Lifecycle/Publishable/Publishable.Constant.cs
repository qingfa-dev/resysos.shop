namespace BuildingBlock.Domain.Modeling.Lifecycle.Publishable;

/// <summary>Unified constants and error definitions for the Publishable concern.</summary>
public static partial class PublishableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the publishable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.entity.required",
                message: "Publishable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the publication timestamp.</summary>
    public static class PublishedAt
    {
        /// <summary>Error when the publication timestamp is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.published_at.required",
                message: "Published date must be specified.");
        }

        /// <summary>Error when the publication timestamp is too far in the future.</summary>
        public static class Future
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.published_at.in_future",
                message:
                    $"Published date cannot be more than {LifecycleConstant.Timestamp.MaxFutureSkew.Value} seconds in the future.");
        }
    }

    /// <summary>Constraints and error definitions for the publication state.</summary>
    public static class State
    {
        /// <summary>Error when the entity is already published.</summary>
        public static class AlreadyPublished
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.already_published",
                message: "Entity is already published.");
        }

        /// <summary>Error when the entity is not published.</summary>
        public static class NotPublished
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.not_published",
                message: "Entity is not published, so it cannot be unpublished.");
        }
    }

    /// <summary>Constraints and error definitions for the publishing actor.</summary>
    public static class PublishedBy
    {
        /// <summary>Error when the actor is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "publishable.published_by.required",
                message: "Published by is required.");
        }

        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = LifecycleConstant.Actor.MinLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "publishable.published_by.too_short",
                message: $"Published by must be at least {Value} characters.");
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = LifecycleConstant.Actor.MaxLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "publishable.published_by.too_long",
                message: $"Published by cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = LifecycleConstant.Actor.Format.Pattern;

            public static readonly Error Error = Error.Validation(
                code: "publishable.actor.invalid",
                message:
                    "Actor must not be empty or contain leading/trailing whitespace.");
        }
    }

    #endregion
}
