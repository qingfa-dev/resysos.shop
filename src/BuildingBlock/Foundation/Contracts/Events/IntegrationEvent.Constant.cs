namespace BuildingBlock.Contracts.Events;

/// <summary>Unified constants and error definitions for integration events.</summary>
public static partial class IntegrationEventConstant
{
    #region Constraints

    public static class Constraints
    {

        /// <summary>Constraints and error definitions for the event schema version.</summary>
        public static class Version
        {
            /// <summary>Minimum supported schema version.</summary>
            public static class Min
            {
                public const int Value = 1;

                public static readonly Error Error = Error.Validation(
                    code: "integration_event.version.invalid",
                    message: $"Integration event version must be at least {Value}.");
            }
        }

        /// <summary>Constraints and error definitions for the event type name.</summary>
        public static class EventType
        {
            /// <summary>Error when the event type name is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "integration_event.type.required",
                    message: "Integration event type must be specified.");
            }
        }

        /// <summary>Constraints and error definitions for a metadata entry.</summary>
        public static class Metadata
        {
            /// <summary>Error when the metadata key is missing.</summary>
            public static class Key
            {
                /// <summary>Error when the key is required but missing.</summary>
                public static class Required
                {
                    public static readonly Error Error = Error.Validation(
                        code: "integration_event.metadata.key.required",
                        message: "Integration event metadata key must be specified.");
                }
            }
        }

    }

    #endregion

    #region Metadata Keys

    /// <summary>Well-known metadata keys used by integration events.</summary>
    public static class MetadataKeys
    {
        /// <summary>Correlation ID for tracking a request across services.</summary>
        public const string CorrelationId = "CorrelationId";

        /// <summary>Causation ID linking this event to the event that caused it.</summary>
        public const string CausationId = "CausationId";

        /// <summary>User ID who triggered the event.</summary>
        public const string UserId = "UserId";

        /// <summary>Tenant ID for multi-tenancy.</summary>
        public const string TenantId = "TenantId";

        /// <summary>Trace ID for distributed tracing.</summary>
        public const string TraceId = "TraceId";

        /// <summary>Span ID for distributed tracing.</summary>
        public const string SpanId = "SpanId";
    }

    #endregion

    #region Defaults

    /// <summary>Default values for integration events.</summary>
    public static class Defaults
    {
        /// <summary>The initial schema version of a new event.</summary>
        public const int Version = IntegrationEventConstant.Constraints.Version.Min.Value;
    }

    #endregion
}
