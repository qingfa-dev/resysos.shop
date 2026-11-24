using BuildingBlock.Functional.Metadata;

namespace BuildingBlock.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that other parts
/// of the same domain (or application layer) should be aware of.
/// </summary>
public interface IDomainEvent : IHasMetadata
{
    /// <summary>
    /// Gets the unique identifier for this event occurrence.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred (UTC).
    /// </summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>
    /// Gets the version of the event schema for serialization compatibility.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Gets the type name of the event for serialization/deserialization.
    /// </summary>
    string EventType { get; }
}
