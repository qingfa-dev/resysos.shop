namespace BuildingBlock.Querying.Compilation;

/// <summary>Unified constants and error definitions for raw filter value conversion.</summary>
public static partial class QueryValueConverterConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for filter values.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for a filter value.</summary>
        public static class Value
        {
            /// <summary>Error when an operator needs a value but none was supplied.</summary>
            public static class Required
            {
                /// <summary>The error returned when a required filter value is missing.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.specification.value.required",
                    message: "A filter value is required for this operator.");
            }

            /// <summary>Error when a raw value cannot be converted to the field type.</summary>
            public static class Invalid
            {
                /// <summary>Error code for an unconvertible filter value.</summary>
                public const string Code = "query.specification.value.invalid";

                /// <summary>Creates a validation error for an unconvertible value.</summary>
                /// <param name="field">The field the value belongs to.</param>
                /// <param name="value">The raw value.</param>
                /// <param name="type">The field type.</param>
                /// <returns>The validation error.</returns>
                public static Error For(string field, string value, Type type) => Error.Validation(
                    Code,
                    $"'{value}' is not a valid {type.Name} value for '{field}'.");
            }
        }
    }

    #endregion

    #region Defaults

    /// <summary>Culture and parsing defaults shared by every conversion.</summary>
    public static class Defaults
    {
        /// <summary>The culture used for every conversion, so values parse invariantly.</summary>
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        /// <summary>The date/time styles used for offset-aware values.</summary>
        public const DateTimeStyles RoundtripStyles = DateTimeStyles.RoundtripKind;

        /// <summary>The date/time styles used for culture-neutral date and time values.</summary>
        public const DateTimeStyles DateStyles = DateTimeStyles.None;
    }

    #endregion
}
