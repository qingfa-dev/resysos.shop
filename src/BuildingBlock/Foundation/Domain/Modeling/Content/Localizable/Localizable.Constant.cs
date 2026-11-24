namespace BuildingBlock.Domain.Modeling.Content.Localizable;

/// <summary>Unified constants and error definitions for the <see cref="Localizable"/> concern.</summary>
public static partial class LocalizableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the localizable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the localizable entity is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "localizable.entity.required",
                message: "Localizable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for a localized property name.</summary>
    public static class Property
    {
        /// <summary>Error when the localized property is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "localizable.property.required",
                message: "Localized property must be specified.");
        }

        /// <summary>Minimum length for a localized property name.</summary>
        public static class MinLength
        {
            public const int Value = 1;

            public static readonly Error Error = Error.Validation(
                code: "localizable.property.too_short",
                message: $"Localized property must be at least {Value} character(s).");
        }

        /// <summary>Maximum length for a localized property name.</summary>
        public static class MaxLength
        {
            public const int Value = 128;

            public static readonly Error Error = Error.Validation(
                code: "localizable.property.too_long",
                message: $"Localized property cannot exceed {Value} characters.");
        }
    }

    /// <summary>Constraints and error definitions for a locale identifier.</summary>
    public static class Locale
    {
        /// <summary>Error when the locale is required but missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "localizable.locale.required",
                message: "Locale must be specified.");
        }

        /// <summary>Minimum length for a locale identifier.</summary>
        public static class MinLength
        {
            public const int Value = 2;

            public static readonly Error Error = Error.Validation(
                code: "localizable.locale.too_short",
                message: $"Locale must be at least {Value} characters.");
        }

        /// <summary>Maximum length for a locale identifier.</summary>
        public static class MaxLength
        {
            public const int Value = 35;

            public static readonly Error Error = Error.Validation(
                code: "localizable.locale.too_long",
                message: $"Locale cannot exceed {Value} characters.");
        }

        /// <summary>Format constraint for a locale identifier.</summary>
        public static class Format
        {
            public const string Pattern = @"^[A-Za-z]{2,8}(?:-[A-Za-z0-9]{2,8})*$";

            public static readonly Error Error = Error.Validation(
                code: "localizable.locale.invalid",
                message: "Locale must be a valid BCP 47 identifier (e.g. \"en\", \"en-US\").");
        }
    }

    /// <summary>Constraints and error definitions for a localized value.</summary>
    public static class Value
    {
        /// <summary>Maximum length for a localized value.</summary>
        public static class MaxLength
        {
            public const int Value = 4000;

            public static readonly Error Error = Error.Validation(
                code: "localizable.value.too_long",
                message: $"Localized value cannot exceed {Value} characters.");
        }

        /// <summary>Error when the localized value is not present.</summary>
        public static class NotFound
        {
            public static readonly Error Error = Error.Validation(
                code: "localizable.value.not_found",
                message: "Localized value is not present.");
        }
    }

    /// <summary>Constraints and error definitions for the localized-value collection.</summary>
    public static class Collection
    {
        /// <summary>Maximum number of (property, locale) translations per entity.</summary>
        public static class MaxTranslations
        {
            public const int Value = 200;

            public static readonly Error Error = Error.Validation(
                code: "localizable.translations.too_many",
                message: $"Cannot store more than {Value} translations.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the <see cref="Localizable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Fallback locale used when no locale is specified by a caller.</summary>
        public const string Locale = "en";
    }

    #endregion
}
