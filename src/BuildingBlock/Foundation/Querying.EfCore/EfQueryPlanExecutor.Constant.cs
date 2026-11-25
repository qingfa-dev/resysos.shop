namespace BuildingBlock.Querying.EfCore;

/// <summary>Unified constants and error definitions for the EF Core plan executor.</summary>
public static partial class EfQueryPlanExecutorConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by the EF Core executor.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for a missing source query.</summary>
        public static class Source
        {
            /// <summary>Error when no source query is supplied.</summary>
            public static class Required
            {
                /// <summary>The error returned when the source query is null.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.ef_core.source.required",
                    message: "A source query is required.");
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
                    code: "query.ef_core.plan.required",
                    message: "A query plan is required.");
            }
        }
    }

    #endregion
}
