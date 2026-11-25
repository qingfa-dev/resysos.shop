namespace BuildingBlock.Querying.Policy;

/// <summary>Unified constants and error definitions for the query context.</summary>
public static partial class QueryContextConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a query context.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for a caller role.</summary>
        public static class Role
        {
            /// <summary>The role must be present (non-blank).</summary>
            public static class Required
            {
                /// <summary>The error returned when a supplied role is null, empty, or whitespace.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.context.role.required",
                    message: "A role must be a non-empty value.");
            }
        }
    }

    #endregion
}
