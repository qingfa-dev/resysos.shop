namespace BuildingBlock.Querying.Security;

/// <summary>Unified constants and error definitions for query security validation.</summary>
public static partial class QuerySecurityConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced before a query executes.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for a field that is not on the allowlist.</summary>
        /// <remarks>
        /// Reuses the field registry's definition so the wire contract
        /// (<c>query.security.field.not_allowed</c>) has a single source of truth.
        /// </remarks>
        public static class Field
        {
            /// <summary>Error code for a field that is not allowed for querying.</summary>
            public const string NotAllowedCode = QueryFieldRegistryConstant.Constraints.Field.NotAllowedCode;

            /// <summary>Creates the error for a field that is not allowed for querying.</summary>
            /// <param name="field">The rejected field name.</param>
            /// <returns>The validation error.</returns>
            public static Error NotAllowed(string? field)
                => QueryFieldRegistryConstant.Constraints.Field.NotAllowed(field);
        }

        /// <summary>Error definitions for a filter operator a field does not allow.</summary>
        public static class Operator
        {
            /// <summary>Error code for a filter operator a field does not allow.</summary>
            public const string NotAllowedCode = "query.security.operator.not_allowed";

            /// <summary>Creates the error for a filter operator a field does not allow.</summary>
            /// <param name="field">The field the operator was applied to.</param>
            /// <param name="operator">The rejected operator.</param>
            /// <returns>The validation error.</returns>
            public static Error NotAllowed(string? field, FilterOperator @operator) => Error.Validation(
                NotAllowedCode,
                $"The operator '{@operator}' is not allowed for the field '{field}'.");
        }

        /// <summary>Error definitions for a filter that breaches a policy cap.</summary>
        public static class Filter
        {
            /// <summary>Error code for a filter nested deeper than the policy allows.</summary>
            public const string TooDeepCode = "query.security.filter.too_deep";

            /// <summary>Creates the error for a filter nested deeper than the policy allows.</summary>
            /// <param name="depth">The measured filter depth.</param>
            /// <param name="maxDepth">The maximum depth the policy allows.</param>
            /// <returns>The validation error.</returns>
            public static Error TooDeep(int depth, int maxDepth) => Error.Validation(
                TooDeepCode,
                $"The filter depth {depth} exceeds the allowed maximum of {maxDepth}.");

            /// <summary>Error code for a filter with more conditions than the policy allows.</summary>
            public const string TooManyConditionsCode = "query.security.filter.too_many_conditions";

            /// <summary>Creates the error for a filter with more conditions than the policy allows.</summary>
            /// <param name="count">The measured condition count.</param>
            /// <param name="maxCount">The maximum condition count the policy allows.</param>
            /// <returns>The validation error.</returns>
            public static Error TooManyConditions(int count, int maxCount) => Error.Validation(
                TooManyConditionsCode,
                $"The filter has {count} conditions, exceeding the allowed maximum of {maxCount}.");
        }

        /// <summary>Error definitions for a sort on a field that is not sortable.</summary>
        public static class Sort
        {
            /// <summary>Error code for a sort on a field that is not sortable.</summary>
            public const string NotAllowedCode = "query.security.sort.not_allowed";

            /// <summary>Creates the error for a sort on a field that is not sortable.</summary>
            /// <param name="field">The field that was sorted on.</param>
            /// <returns>The validation error.</returns>
            public static Error NotAllowed(string? field) => Error.Validation(
                NotAllowedCode,
                $"The field '{field}' is not allowed for sorting.");
        }
    }

    #endregion
}
