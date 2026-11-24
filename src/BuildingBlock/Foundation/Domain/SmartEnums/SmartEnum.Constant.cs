namespace BuildingBlock.Domain.SmartEnums;

/// <summary>Constants and error definitions for the smart enumeration concern.</summary>
public static partial class SmartEnumConstant
{
    #region Constraints



    /// <summary>Constraints for the numeric identifier.</summary>
    public static class Id
    {
        /// <summary>The smallest allowed identifier.</summary>
        public static class Min
        {
            public const int Value = 0;

            public static readonly Error Error = Error.Validation(
                code: "smart_enum.id.invalid",
                message: $"Smart enum identifier must be at least {Value}.");
        }
    }

    /// <summary>Constraints for the display name.</summary>
    public static class Name
    {
        /// <summary>Maximum length for a display name.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "smart_enum.name.too_long",
                message: $"Smart enum name cannot exceed {Value} characters.");
        }

        /// <summary>Error when the display name is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "smart_enum.name.required",
                message: "Smart enum name must be specified.");
        }
    }

    /// <summary>Errors raised when the registry contains duplicates.</summary>
    public static class Duplicate
    {
        /// <summary>Error when two members share an identifier.</summary>
        public static class Id
        {
            public static readonly Error Error = Error.Conflict(
                code: "smart_enum.id.duplicate",
                message: "Smart enum members must have unique identifiers.");
        }

        /// <summary>Error when two members share a name.</summary>
        public static class Name
        {
            public static readonly Error Error = Error.Conflict(
                code: "smart_enum.name.duplicate",
                message: "Smart enum members must have unique names.");
        }
    }

    /// <summary>Errors raised when a lookup finds no member.</summary>
    public static class Lookup
    {
        /// <summary>Error when no member has the requested identifier.</summary>
        public static class IdNotFound
        {
            public static readonly Error Error = Error.NotFound(
                code: "smart_enum.id.not_found",
                message: "No smart enum member has the requested identifier.");
        }

        /// <summary>Error when no member has the requested name.</summary>
        public static class NameNotFound
        {
            public static readonly Error Error = Error.NotFound(
                code: "smart_enum.name.not_found",
                message: "No smart enum member has the requested name.");
        }
    }

    #endregion
}
