
/// <summary>
/// Constants for well-known metadata keys used in domain events.
/// </summary>
public static class DomainEventConstant
{
    public static class Metadata
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
}