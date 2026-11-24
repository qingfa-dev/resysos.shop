namespace BuildingBlock.Domain.Modeling.Foundation.Versionable;

/// <summary>Unified constants and error definitions for the Versionable concern.</summary>
public static partial class VersionableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the versionable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity itself is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "versionable.entity.required",
                message: "Versionable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the version value.</summary>
    public static class Version
    {
        /// <summary>Minimum allowed version value.</summary>
        public static class Min
        {
            public const long Value = 0;

            public static readonly Error Error = Error.Validation(
                code: "versionable.version.negative",
                message: $"Version must not be negative (minimum is {Value}).");
        }

        /// <summary>Maximum allowed version value.</summary>
        public static class Max
        {
            public const long Value = long.MaxValue;

            public static readonly Error Error = Error.Validation(
                code: "versionable.version.overflow",
                message: $"Version is already at its maximum value of {Value}.");
        }

        /// <summary>Error when the version falls outside the allowed range.</summary>
        public static class Range
        {
            public static readonly Error Error = Error.Validation(
                code: "versionable.version.out_of_range",
                message: $"Version must be between {Min.Value} and {Max.Value}.");
        }

        /// <summary>Error when the entity version does not match the expected version.</summary>
        public static class Conflict
        {
            /// <summary>Builds a concurrency-conflict error for the given versions.</summary>
            /// <param name="expected">The version the caller expects.</param>
            /// <param name="actual">The version the entity currently carries.</param>
            /// <returns>A conflict error describing the mismatch.</returns>
            public static Error Concurrency(long expected, long actual) =>
                Error.Conflict(
                    code: "versionable.version.concurrency_conflict",
                    message: $"Version conflict: expected '{expected}' but found '{actual}'.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the Versionable concern.</summary>
    public static class Defaults
    {
        /// <summary>Initial version assigned to new entities.</summary>
        public const long InitialVersion = 0;

        /// <summary>Amount added to the version by a single bump.</summary>
        public const long Increment = 1;
    }

    #endregion
}
