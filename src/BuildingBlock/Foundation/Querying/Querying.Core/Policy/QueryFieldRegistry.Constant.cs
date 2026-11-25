namespace BuildingBlock.Querying.Policy;

/// <summary>Unified constants and error definitions for the field registry.</summary>
public static partial class QueryFieldRegistryConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the field registry.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for a queryable field.</summary>
        /// <remarks>
        /// This is the single source of truth for the <c>query.security.field.not_allowed</c> wire
        /// contract; <c>QuerySecurityConstant.Constraints.Field</c> reuses this definition.
        /// </remarks>
        public static class Field
        {
            /// <summary>Error code for a field that is not on the allowlist.</summary>
            public const string NotAllowedCode = "query.security.field.not_allowed";

            /// <summary>Creates the error for a field that is not on the allowlist.</summary>
            /// <param name="field">The rejected field name.</param>
            /// <returns>The validation error.</returns>
            public static Error NotAllowed(string? field) => Error.Validation(
                NotAllowedCode,
                $"The field '{field}' is not allowed for querying.");
        }
    }

    #endregion
}
