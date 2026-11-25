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

        /// <summary>Error definitions for a filter operator.</summary>
        public static class Operator
        {
            /// <summary>Error code for an operator that is not supported for a field.</summary>
            public const string Code = "filter.operator.unsupported";

            /// <summary>Creates a validation error for an unsupported filter operator.</summary>
            /// <param name="name">The operator name that was rejected.</param>
            /// <returns>The validation error.</returns>
            public static Error Unsupported(string name)
                => Error.Validation(Code, $"The filter operator '{name}' is not supported.");
        }
    }

    #endregion

    #region Operators

    /// <summary>Canonical word names of the filter operators.</summary>
    public static class Operators
    {
        /// <summary>Word name of <see cref="FilterOperator.Eq"/>.</summary>
        public const string Eq = "eq";

        /// <summary>Word name of <see cref="FilterOperator.Ne"/>.</summary>
        public const string Ne = "neq";

        /// <summary>Word name of <see cref="FilterOperator.Gt"/>.</summary>
        public const string Gt = "gt";

        /// <summary>Word name of <see cref="FilterOperator.Gte"/>.</summary>
        public const string Gte = "gte";

        /// <summary>Word name of <see cref="FilterOperator.Lt"/>.</summary>
        public const string Lt = "lt";

        /// <summary>Word name of <see cref="FilterOperator.Lte"/>.</summary>
        public const string Lte = "lte";

        /// <summary>Word name of <see cref="FilterOperator.Contains"/>.</summary>
        public const string Contains = "contains";

        /// <summary>Word name of <see cref="FilterOperator.StartsWith"/>.</summary>
        public const string StartsWith = "startswith";

        /// <summary>Word name of <see cref="FilterOperator.EndsWith"/>.</summary>
        public const string EndsWith = "endswith";

        /// <summary>Word name of <see cref="FilterOperator.In"/>.</summary>
        public const string In = "in";

        /// <summary>Word name of <see cref="FilterOperator.IsNull"/>.</summary>
        public const string IsNull = "isnull";

        /// <summary>Word name of <see cref="FilterOperator.IsNotNull"/>.</summary>
        public const string IsNotNull = "isnotnull";

        /// <summary>Word name of <see cref="FilterOperator.NotIn"/>.</summary>
        public const string NotIn = "notin";

        /// <summary>Word name of <see cref="FilterOperator.Between"/>.</summary>
        public const string Between = "between";

        /// <summary>Word name of <see cref="FilterOperator.Any"/>.</summary>
        public const string Any = "any";

        /// <summary>Word name of <see cref="FilterOperator.All"/>.</summary>
        public const string All = "all";
    }

    #endregion

    #region Group Operators

    /// <summary>Canonical word names of the boolean group operators.</summary>
    public static class GroupOperators
    {
        /// <summary>Word name of <see cref="FilterGroupOperator.And"/>.</summary>
        public const string And = "and";

        /// <summary>Word name of <see cref="FilterGroupOperator.Or"/>.</summary>
        public const string Or = "or";
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
