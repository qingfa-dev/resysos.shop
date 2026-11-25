namespace BuildingBlock.Querying.Facade;

/// <summary>Unified constants and error definitions for the query engine.</summary>
public static partial class QueryEngineConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by the query engine.</summary>
    public static class Constraints
    {
        /// <summary>Error definitions for a missing query policy.</summary>
        public static class Policy
        {
            /// <summary>The policy must be present (non-null).</summary>
            public static class Required
            {
                /// <summary>The error returned when no policy is supplied.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.engine.policy.required",
                    message: "A query policy is required.");
            }
        }

        /// <summary>Error definitions for a missing plan executor.</summary>
        public static class Executor
        {
            /// <summary>The executor must be present (non-null).</summary>
            public static class Required
            {
                /// <summary>The error returned when no plan executor is supplied.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.engine.executor.required",
                    message: "A query plan executor is required.");
            }
        }

        /// <summary>Error definitions for a missing service collection.</summary>
        public static class Services
        {
            /// <summary>The service collection must be present (non-null).</summary>
            public static class Required
            {
                /// <summary>The error returned when no service collection is supplied.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.engine.services.required",
                    message: "A service collection is required.");
            }
        }

        /// <summary>Error definitions for an engine that cannot be resolved.</summary>
        public static class Engine
        {
            /// <summary>The engine must be registered with the supplied services.</summary>
            public static class NotRegistered
            {
                /// <summary>The error returned when no engine is registered for the services.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "query.engine.not_registered",
                    message: "No query engine is registered for the supplied services.");
            }
        }
    }

    #endregion
}
