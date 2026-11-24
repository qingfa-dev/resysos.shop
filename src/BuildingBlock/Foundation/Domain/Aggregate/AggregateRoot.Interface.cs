using BuildingBlock.Domain.Events;
using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Domain.Aggregate;

/// <summary>
/// Non-generic interface for aggregates that can raise domain events.
/// </summary>
public interface IAggregateRoot : IAggregateRoot<Guid>
{
}

/// <summary>
/// Generic interface for aggregates that can raise domain events.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public interface IAggregateRoot<TId> : IIdentifiable<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets the collection of domain events raised by this aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    void ClearDomainEvents();

    /// <summary>
    /// Adds a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    void AddDomainEvent(IDomainEvent domainEvent);
}