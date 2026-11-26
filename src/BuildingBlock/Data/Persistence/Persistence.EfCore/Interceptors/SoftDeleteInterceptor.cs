using BuildingBlock.Clock.Abstractions;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;
using BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;
using BuildingBlock.Persistence.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlock.Persistence.EfCore.Interceptors;

/// <summary>
/// Converts hard deletes of <see cref="ISoftDeletable"/> entities into an update that marks the
/// row deleted and records the deletion metadata.
/// </summary>
/// <remarks>Initializes a new interceptor.</remarks>
/// <param name="clock">
/// The clock used for the deletion timestamp; when null, defaults to the system clock.
/// </param>
public sealed class SoftDeleteInterceptor(IClock? clock = null) : SaveChangesInterceptor
{
    private readonly IClock? _clock = clock;

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            ApplySoftDelete(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            ApplySoftDelete(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplySoftDelete(DbContext context)
    {
        List<EntityEntry> deletedEntries = context.ChangeTracker
            .Entries()
            .Where(entry => entry.State == EntityState.Deleted
                && entry.Entity is ISoftDeletable)
            .ToList();

        // TODO: consider create a services to retrieve the current user for DeletedBy instead of using CreatedBy
        foreach (EntityEntry entry in deletedEntries)
        {
            var entity = (ISoftDeletable)entry.Entity;

            entry.State = EntityState.Modified;
            entity.IsDeleted = true;
            entity.DeletedAtUtc = _clock?.UtcNow
                ?? (context as ModuleDbContext)?.InfrastructureTimeSource.GetUtcNow()
                ?? DateTimeOffset.UtcNow;
            entity.DeletedBy = (entry.Entity as ICreatable)?.CreatedBy ?? string.Empty;
        }
    }
}
