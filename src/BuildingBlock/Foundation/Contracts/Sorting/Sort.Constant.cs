namespace BuildingBlock.Contracts.Sorting;

/// <summary>Unified constants and error definitions for the sorting concern.</summary>
public static partial class SortConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for sort clauses.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the field name.</summary>
        public static class Field
        {
            /// <summary>Error when the field name is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "sort.field.required",
                    message: "A sort field name is required.");
            }
        }

        /// <summary>Error for a malformed sort clause.</summary>
        public static class Clause
        {
            public const string Code = "query.invalid_sort";

            /// <summary>Creates a validation error for a malformed sort clause.</summary>
            /// <param name="message">The human-readable reason.</param>
            /// <returns>The validation error.</returns>
            public static Error Invalid(string message) => Error.Validation(Code, message);
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for sorting.</summary>
    public static class Defaults
    {
        /// <summary>Default sort direction.</summary>
        public const SortDirection Direction = SortDirection.Ascending;

        /// <summary>Character separating sort clauses.</summary>
        public const char ClauseSeparator = ',';

        /// <summary>Character prefixing a descending sort.</summary>
        public const char DescendingPrefix = '-';

        /// <summary>Character prefixing an ascending sort.</summary>
        public const char AscendingPrefix = '+';

        /// <summary>Suffix marking an ascending sort.</summary>
        public const string AscendingSuffix = "asc";

        /// <summary>Suffix marking a descending sort.</summary>
        public const string DescendingSuffix = "desc";
    }

    #endregion
}
