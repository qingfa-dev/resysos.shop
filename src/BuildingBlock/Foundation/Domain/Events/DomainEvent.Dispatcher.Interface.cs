using BuildingBlock.Domain.Aggregate;

namespace BuildingBlock.Domain.Events;

/// <summary>
/// Interface for dispatching domain events to handlers.
/// Implementations can use MediatR, custom handlers, or direct invocation.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event asynchronously.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events asynchronously.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches all domain events from an aggregate and clears them.
    /// </summary>
    /// <param name="aggregate">The aggregate containing domain events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    Task DispatchAndClearAsync(IAggregateRoot aggregate, CancellationToken cancellationToken = default);
}

