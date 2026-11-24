namespace BuildingBlock.Domain.Modeling.Foundation.Referenceable;

/// <summary>Unified constants and error definitions for the Referenceable concern.</summary>
public static partial class ReferenceableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the referenceable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity itself is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "referenceable.entity.required",
                message: "Referenceable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a reference string.</summary>
    public static class Reference
    {
        /// <summary>Error when the reference is missing or whitespace.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "referenceable.reference.required",
                message:
                    $"Reference must be specified ({MinLength.Value}-{MaxLength.Value} characters).");
        }

        /// <summary>Minimum allowed length for a reference string.</summary>
        public static class MinLength
        {
            public const int Value = 1;
        }

        /// <summary>Maximum allowed length for a reference string.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "referenceable.reference.too_long",
                message: $"Reference cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint rejecting leading or trailing whitespace.</summary>
        public static class Trimmed
        {
            public const string Pattern = @"^\S(?:.*\S)?$";

            public static readonly Error Error = Error.Validation(
                code: "referenceable.reference.untrimmed",
                message: "Reference must not have leading or trailing whitespace.");
        }

        /// <summary>Format constraint for the allowed character set.</summary>
        public static class Charset
        {
            public const string Pattern = @"^[A-Za-z0-9\-_/#:. ]+$";

            public static readonly Error Error = Error.Validation(
                code: "referenceable.reference.invalid_charset",
                message: "Reference contains invalid characters. Allowed characters: letters, digits, space, '-', '_', '/', '#', '.', ':'.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the Referenceable concern.</summary>
    public static class Defaults
    {
        /// <summary>Empty reference value used when clearing.</summary>
        public const string Empty = "";
    }

    #endregion
}
