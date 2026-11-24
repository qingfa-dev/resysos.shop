using BuildingBlock.Domain.Entity;
using BuildingBlock.Domain.Events;

namespace BuildingBlock.Domain.Aggregate;

/// <summary>
/// Base class for aggregate roots with domain event support.
/// An aggregate root is the entry point to a consistency boundary.
/// </summary>
public abstract class AggregateRoot : Entity<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// </summary>
    protected AggregateRoot() : base(Guid.NewGuid()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class with the specified id.
    /// </summary>
    protected AggregateRoot(Guid id) : base(id) { }

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
    /// <typeparam name="TEvent">The type of domain event.</typeparam>
    /// <param name="eventFactory">Factory function to create the event with aggregate context.</param>
    protected void RaiseEvent<TEvent>(Func<Guid, TEvent> eventFactory)
        where TEvent : DomainEvent
    {
        var domainEvent = eventFactory(Id);
        AddDomainEvent(domainEvent);
    }
}

/// <summary>
/// Base class for aggregate roots with a typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    protected AggregateRoot() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified id.
    /// </summary>
    protected AggregateRoot(TId id) : base(id) { }

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
    /// <typeparam name="TEvent">The type of domain event.</typeparam>
    /// <param name="eventFactory">Factory function to create the event with aggregate context.</param>
    protected void RaiseEvent<TEvent>(Func<TId, TEvent> eventFactory)
        where TEvent : DomainEvent
    {
        var domainEvent = eventFactory(Id);
        AddDomainEvent(domainEvent);
    }
}