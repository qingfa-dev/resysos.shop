using BuildingBlock.Functional.Metadata;

namespace BuildingBlock.Domain.Events;


/// <summary>
/// Base class for domain events with standard metadata.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public int Version { get; init; } = 1;

    /// <inheritdoc />
    public string EventType { get; init; } = string.Empty;

    /// <inheritdoc />
    public IMetadata Metadata { get; init; } = DefaultMetadata.Empty();

    /// <summary>
    /// Protected constructor for derived events.
    /// </summary>
    protected DomainEvent()
    {
        EventType = GetType().Name;
    }

    /// <summary>
    /// Creates a new event with additional metadata.
    /// </summary>
    public T WithMetadata<T>(string key, object value) where T : DomainEvent
    {
        var newMetadata = Metadata.Count == 0
            ? DefaultMetadata.Empty()
            : DefaultMetadata.From(Metadata);
        newMetadata.WithMetadata(key, value);
        return (T)(object)(this with { Metadata = newMetadata });
    }

    /// <summary>
    /// Creates a new event with correlation metadata.
    /// </summary>
    public T WithCorrelation<T>(Guid correlationId, Guid? causationId = null) where T : DomainEvent
        => WithMetadata<T>(DomainEventConstant.Metadata.CorrelationId, correlationId)
            .WithMetadata(DomainEventConstant.Metadata.CausationId, causationId ?? correlationId);

}