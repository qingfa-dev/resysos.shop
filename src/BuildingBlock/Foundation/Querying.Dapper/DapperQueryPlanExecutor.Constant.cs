namespace BuildingBlock.Querying.Dapper;

/// <summary>Unified constants and error definitions for the Dapper plan executor.</summary>
public static partial class DapperQueryPlanExecutorConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by the Dapper executor.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for a missing source sequence.</summary>
        public static class Source
        {
            /// <summary>Error when no source sequence is supplied.</summary>
            public static class Required
            {
                /// <summary>The error returned when the source sequence is null.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.dapper.source.required",
                    message: "A source sequence is required.");
            }
        }

        /// <summary>Error definitions for a missing query plan.</summary>
        public static class Plan
        {
            /// <summary>Error when no query plan is supplied.</summary>
            public static class Required
            {
                /// <summary>The error returned when the query plan is null.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.dapper.plan.required",
                    message: "A query plan is required.");
            }
        }
    }

    #endregion
}
