using BuildingBlock.Domain.Modeling.Foundation.Versionable;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Persistence.EfCore.Extensions;

/// <summary>
/// Marks the <see cref="IVersionable.Version"/> property as an optimistic concurrency
/// token, so an update whose version no longer matches fails instead of overwriting.
/// </summary>
public static class ConcurrencyExtensions
{
    /// <summary>Applies the optimistic concurrency mapping to every versioned entity.</summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same model builder.</returns>
    public static ModelBuilder ConfigureOptimisticConcurrency(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IVersionable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var version = entityType.FindProperty(nameof(IVersionable.Version));
            if (version is not null)
            {
                version.IsConcurrencyToken = true;
            }
        }

        return modelBuilder;
    }
}
