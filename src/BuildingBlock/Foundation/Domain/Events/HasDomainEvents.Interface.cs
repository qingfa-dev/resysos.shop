namespace BuildingBlock.Domain.Events;

/// <summary>
/// Interface for entities that can raise domain events.
/// This is a lighter alternative to IAggregateRoot for entities within an aggregate
/// that need to raise events (which are then delegated to the aggregate root).
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    void ClearDomainEvents();

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    void AddDomainEvent(IDomainEvent domainEvent);
}

/// <summary>
/// Base class for entities that can raise domain events.
/// Events are typically collected by the aggregate root.
/// </summary>
public abstract class EntityWithDomainEvents : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Raises a domain event (convenience method).
    /// </summary>
    protected void RaiseEvent<TEvent>(Func<TEvent> eventFactory)
        where TEvent : DomainEvent
    {
        var domainEvent = eventFactory();
        AddDomainEvent(domainEvent);
    }
}