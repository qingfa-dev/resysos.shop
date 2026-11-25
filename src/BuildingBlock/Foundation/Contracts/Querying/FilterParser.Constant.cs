using BuildingBlock.Contracts.Filtering;

namespace BuildingBlock.Contracts.Querying;

/// <summary>Constants and error definitions for <see cref="FilterParser"/>.</summary>
public static class FilterParserConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for filter parsing.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for the filter parser.</summary>
        public static class Parser
        {
            /// <summary>Error code for a filter expression that does not match the grammar.</summary>
            public const string Code = FilterConstant.Constraints.Clause.Code;

            /// <summary>Creates a validation error for a filter expression that does not match the grammar.</summary>
            /// <param name="position">The zero-based position the parser stopped at.</param>
            /// <param name="message">The human-readable reason.</param>
            /// <returns>The validation error.</returns>
            public static Error Malformed(int position, string message)
                => Error.Validation(Code, $"{message} (position {position}).");
        }
    }

    #endregion
}
