namespace BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

/// <summary>Unified constants and error definitions for the Modifiable concern.</summary>
public static partial class ModifiableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the modifiable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.entity.required",
                message: "Modifiable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the modification state.</summary>
    public static class State
    {
        /// <summary>Error when the entity has no creation audit metadata.</summary>
        public static class NotInitialized
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.not_initialized",
                message:
                    "Entity has no creation audit metadata; initialize it before marking it modified.");
        }
    }

    /// <summary>Constraints and error definitions for the modification timestamp.</summary>
    public static class ModifiedAt
    {
        /// <summary>Error when the modification timestamp is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_at.required",
                message: "Modified date must be specified.");
        }

        /// <summary>Error when the modification timestamp is too far in the future.</summary>
        public static class Future
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_at.in_future",
                message:
                    $"Modified date cannot be more than {LifecycleConstant.Timestamp.MaxFutureSkew.Value} seconds in the future.");
        }

        /// <summary>Error when the modification timestamp precedes the creation timestamp.</summary>
        public static class BeforeCreatedAt
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_at.invalid_range",
                message: "Modified date cannot be earlier than created date.");
        }
    }

    /// <summary>Constraints and error definitions for the modifying actor.</summary>
    public static class ModifiedBy
    {
        /// <summary>Error when the actor is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_by.required",
                message: "Modified by is required when the entity has been modified.");
        }

        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = LifecycleConstant.Actor.MinLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_by.too_short",
                message: $"Modified by must be at least {Value} characters.");
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = LifecycleConstant.Actor.MaxLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "modifiable.modified_by.too_long",
                message: $"Modified by cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = LifecycleConstant.Actor.Format.Pattern;

            public static readonly Error Error = Error.Validation(
                code: "modifiable.actor.invalid",
                message:
                    "Actor must not be empty or contain leading/trailing whitespace.");
        }
    }

    #endregion
}
