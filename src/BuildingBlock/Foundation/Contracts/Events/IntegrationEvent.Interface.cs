namespace BuildingBlock.Contracts.Events;

/// <summary>Interface for integration events.</summary>
public interface IIntegrationEvent : IHasMetadata
{
    /// <summary>Gets the unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>Gets the timestamp when the event occurred (UTC).</summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>Gets the version of the event schema for serialization compatibility.</summary>
    int Version { get; }

    /// <summary>Gets the type name of the event for serialization/deserialization.</summary>
    string EventType { get; }

    /// <summary>Gets the correlation ID for tracking a request across services.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation ID linking this event to the event that caused it.</summary>
    Guid? CausationId { get; }
}
