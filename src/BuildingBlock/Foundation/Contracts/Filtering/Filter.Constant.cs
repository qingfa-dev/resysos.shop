namespace BuildingBlock.Contracts.Filtering;

/// <summary>Unified constants and error definitions for the filtering concern.</summary>
public static partial class FilterConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for filter clauses.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the field name.</summary>
        public static class Field
        {
            /// <summary>Error when the field name is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "filter.field.required",
                    message: "A filter field name is required.");
            }
        }

        /// <summary>Error for a malformed filter clause.</summary>
        public static class Clause
        {
            public const string Code = "query.invalid_filter";

            /// <summary>Creates a validation error for a malformed filter clause.</summary>
            /// <param name="message">The human-readable reason.</param>
            /// <returns>The validation error.</returns>
            public static Error Invalid(string message) => Error.Validation(Code, message);
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for filtering.</summary>
    public static class Defaults
    {
        /// <summary>Default comparison operator.</summary>
        public const FilterOperator Operator = FilterOperator.Eq;

        /// <summary>Character separating filter clauses.</summary>
        public const char ClauseSeparator = ',';

        /// <summary>Character separating the values of an <see cref="FilterOperator.In"/> clause.</summary>
        public const char ValueSeparator = '|';

        /// <summary>Character that quotes literal text.</summary>
        public const char Quote = '\'';
    }

    #endregion
}
