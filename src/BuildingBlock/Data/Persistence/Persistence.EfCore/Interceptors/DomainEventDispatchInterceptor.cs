using BuildingBlock.Domain.Aggregate;
using BuildingBlock.Domain.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlock.Persistence.EfCore.Interceptors;

/// <summary>
/// Dispatches the domain events raised by tracked aggregate roots before changes are saved, so
/// handlers observe the same transaction as the state change that produced them.
/// </summary>
/// <remarks>Initializes a new interceptor.</remarks>
/// <param name="dispatcher">
/// The dispatcher used for raised events; when null, dispatch is disabled.
/// </param>
public sealed class DomainEventDispatchInterceptor(IDomainEventDispatcher? dispatcher) : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher? _dispatcher = dispatcher;

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (_dispatcher is not null && eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken)
                .ConfigureAwait(false);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task DispatchDomainEventsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        List<IAggregateRoot> aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (IAggregateRoot aggregate in aggregates)
        {
            await _dispatcher!.DispatchAndClearAsync(aggregate, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
