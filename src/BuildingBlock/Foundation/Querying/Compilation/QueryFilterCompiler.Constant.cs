namespace BuildingBlock.Querying.Compilation;

/// <summary>Unified constants and error definitions for the filter expression compiler.</summary>
public static partial class QueryFilterCompilerConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for filter compilation.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for filter operators.</summary>
        public static class Operator
        {
            /// <summary>Error when an operator cannot be applied to a field type.</summary>
            public static class Unsupported
            {
                /// <summary>Error code for an operator that cannot be applied to a field type.</summary>
                public const string Code = "query.specification.operator.unsupported";

                /// <summary>Creates a not-supported error for an operator and field type.</summary>
                /// <param name="operator">The unsupported operator.</param>
                /// <param name="field">The field the operator applies to.</param>
                /// <param name="type">The field type.</param>
                /// <returns>The not-supported error.</returns>
                public static Error For(FilterOperator @operator, string field, Type type) => Error.NotSupported(
                    Code,
                    $"Operator '{@operator}' is not supported for '{field}' of type '{type.Name}'.");
            }
        }
    }

    #endregion

    #region Defaults

    /// <summary>Method names and shapes used while building comparison expressions.</summary>
    public static class Defaults
    {
        /// <summary>The name of <see cref="string.Contains(string)"/>.</summary>
        public const string Contains = nameof(string.Contains);

        /// <summary>The name of <see cref="string.StartsWith(string)"/>.</summary>
        public const string StartsWith = nameof(string.StartsWith);

        /// <summary>The name of <see cref="string.EndsWith(string)"/>.</summary>
        public const string EndsWith = nameof(string.EndsWith);

        /// <summary>The name of <see cref="string.ToLower()"/>.</summary>
        public const string ToLower = nameof(string.ToLower);

        /// <summary>The name of <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)"/>.</summary>
        public const string EnumerableContains = nameof(Enumerable.Contains);
    }

    #endregion
}
