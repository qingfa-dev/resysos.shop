namespace BuildingBlock.Contracts.Events;

/// <summary>
/// Base class for integration events published to external systems / other bounded contexts.
/// Integration events are versioned, serializable, and represent facts that have occurred.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    #region State

    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public int Version { get; init; } = IntegrationEventConstant.Defaults.Version;

    /// <inheritdoc />
    public string EventType { get; init; } = string.Empty;

    /// <inheritdoc />
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public Guid? CausationId { get; init; }

    /// <inheritdoc />
    public IMetadata Metadata { get; init; } = DefaultMetadata.Empty();

    #endregion

    #region Construction

    /// <summary>Protected constructor for derived events.</summary>
    protected IntegrationEvent()
    {
        EventType = GetType().Name;
    }

    #endregion

    #region Enrichment

    /// <summary>Creates a new event with additional metadata.</summary>
    public T WithMetadata<T>(string key, object value) where T : IntegrationEvent
    {
        var newMetadata = Metadata.Count == 0
            ? DefaultMetadata.Empty()
            : DefaultMetadata.From(Metadata);
        newMetadata.WithMetadata(key, value);
        return (T)(object)(this with { Metadata = newMetadata });
    }

    /// <summary>Creates a new event carrying correlation and causation identifiers.</summary>
    public T WithCorrelation<T>(Guid correlationId, Guid? causationId = null)
        where T : IntegrationEvent
        => (T)(object)(this with { CorrelationId = correlationId, CausationId = causationId });

    #endregion
}
