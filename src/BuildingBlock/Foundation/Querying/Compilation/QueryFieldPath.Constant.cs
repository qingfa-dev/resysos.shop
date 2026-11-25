namespace BuildingBlock.Querying.Compilation;

/// <summary>Unified constants and error definitions for resolved field paths.</summary>
public static partial class QueryFieldPathConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a resolved field path.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for a field path.</summary>
        public static class Field
        {
            /// <summary>Error code for a field path that does not resolve to a member chain.</summary>
            public const string UnknownCode = "query.specification.field.unknown";

            /// <summary>Creates a validation error for an unresolved field path.</summary>
            /// <param name="type">The entity type name.</param>
            /// <param name="field">The unresolved field path.</param>
            /// <returns>The validation error.</returns>
            public static Error Unknown(string type, string field) => Error.Validation(
                UnknownCode,
                $"'{field}' is not a public property of '{type}'.");
        }
    }

    #endregion
}
