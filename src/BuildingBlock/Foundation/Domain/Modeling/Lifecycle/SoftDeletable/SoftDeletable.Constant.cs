namespace BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

/// <summary>Unified constants and error definitions for the SoftDeletable concern.</summary>
public static partial class SoftDeletableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the soft-deletable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.entity.required",
                message: "Soft-deletable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the deletion timestamp.</summary>
    public static class DeletedAt
    {
        /// <summary>Error when the deletion timestamp is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.deleted_at.required",
                message: "Deleted date must be specified.");
        }

        /// <summary>Error when the deletion timestamp is too far in the future.</summary>
        public static class Future
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.deleted_at.in_future",
                message:
                    $"Deleted date cannot be more than {LifecycleConstant.Timestamp.MaxFutureSkew.Value} seconds in the future.");
        }
    }

    /// <summary>Constraints and error definitions for the deletion state.</summary>
    public static class State
    {
        /// <summary>Error when the entity is already deleted.</summary>
        public static class AlreadyDeleted
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.already_deleted",
                message: "Entity is already deleted.");
        }

        /// <summary>Error when the entity is not deleted.</summary>
        public static class NotDeleted
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.not_deleted",
                message: "Entity is not deleted, so it cannot be restored.");
        }
    }

    /// <summary>Constraints and error definitions for the deleting actor.</summary>
    public static class DeletedBy
    {
        /// <summary>Error when the actor is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.deleted_by.required",
                message: "Deleted by is required.");
        }

        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = LifecycleConstant.Actor.MinLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.deleted_by.too_short",
                message: $"Deleted by must be at least {Value} characters.");
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = LifecycleConstant.Actor.MaxLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.deleted_by.too_long",
                message: $"Deleted by cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = LifecycleConstant.Actor.Format.Pattern;

            public static readonly Error Error = Error.Validation(
                code: "soft_deletable.actor.invalid",
                message:
                    "Actor must not be empty or contain leading/trailing whitespace.");
        }
    }

    #endregion
}
