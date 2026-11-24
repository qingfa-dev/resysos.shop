namespace BuildingBlock.Domain.Modeling.Lifecycle.Activatable;

/// <summary>Unified constants and error definitions for the Activatable concern.</summary>
public static partial class ActivatableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the activatable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.entity.required",
                message: "Activatable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the activation timestamp.</summary>
    public static class ActivatedAt
    {
        /// <summary>Error when the activation timestamp is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.activated_at.required",
                message: "Activated date must be specified.");
        }

        /// <summary>Error when the activation timestamp is too far in the future.</summary>
        public static class Future
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.activated_at.in_future",
                message:
                    $"Activated date cannot be more than {LifecycleConstant.Timestamp.MaxFutureSkew.Value} seconds in the future.");
        }
    }

    /// <summary>Constraints and error definitions for the activation state.</summary>
    public static class State
    {
        /// <summary>Error when the entity is already active.</summary>
        public static class AlreadyActive
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.already_active",
                message: "Entity is already active.");
        }

        /// <summary>Error when the entity is not active.</summary>
        public static class NotActive
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.not_active",
                message: "Entity is not active, so it cannot be deactivated.");
        }
    }

    /// <summary>Constraints and error definitions for the activating actor.</summary>
    public static class ActivatedBy
    {
        /// <summary>Error when the actor is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "activatable.activated_by.required",
                message: "Activated by is required.");
        }

        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = LifecycleConstant.Actor.MinLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "activatable.activated_by.too_short",
                message: $"Activated by must be at least {Value} characters.");
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = LifecycleConstant.Actor.MaxLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "activatable.activated_by.too_long",
                message: $"Activated by cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = LifecycleConstant.Actor.Format.Pattern;

            public static readonly Error Error = Error.Validation(
                code: "activatable.actor.invalid",
                message:
                    "Actor must not be empty or contain leading/trailing whitespace.");
        }
    }

    #endregion
}
