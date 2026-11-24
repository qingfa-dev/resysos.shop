namespace BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

/// <summary>Unified constants and error definitions for the Creatable concern.</summary>
public static partial class CreatableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the creatable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "creatable.entity.required",
                message: "Creatable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the creation timestamp.</summary>
    public static class CreatedAt
    {
        /// <summary>Error when the creation timestamp is not specified.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "creatable.created_at.required",
                message: "Created date must be specified.");
        }

        /// <summary>Error when the creation timestamp is too far in the future.</summary>
        public static class Future
        {
            public static readonly Error Error = Error.Validation(
                code: "creatable.created_at.in_future",
                message:
                    $"Created date cannot be more than {LifecycleConstant.Timestamp.MaxFutureSkew.Value} seconds in the future.");
        }

        /// <summary>Error when the entity is already initialized.</summary>
        public static class AlreadyInitialized
        {
            public static readonly Error Error = Error.Validation(
                code: "creatable.created_at.already_initialized",
                message:
                    "Creation audit metadata is already set; it cannot be initialized twice.");
        }
    }

    /// <summary>Constraints and error definitions for the creating actor.</summary>
    public static class CreatedBy
    {
        /// <summary>Error when the actor is required but not provided.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "creatable.created_by.required",
                message: "Created by is required.");
        }

        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = LifecycleConstant.Actor.MinLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "creatable.created_by.too_short",
                message: $"Created by must be at least {Value} characters.");
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = LifecycleConstant.Actor.MaxLength.Value;

            public static readonly Error Error = Error.Validation(
                code: "creatable.created_by.too_long",
                message: $"Created by cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = LifecycleConstant.Actor.Format.Pattern;

            public static readonly Error Error = Error.Validation(
                code: "creatable.actor.invalid",
                message:
                    "Actor must not be empty or contain leading/trailing whitespace.");
        }
    }

    #endregion
}
