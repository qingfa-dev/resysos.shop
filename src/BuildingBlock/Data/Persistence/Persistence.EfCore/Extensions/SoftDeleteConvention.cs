using BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace BuildingBlock.Persistence.EfCore.Extensions;

/// <summary>
/// Applies the soft-delete query filter to every entity implementing
/// <see cref="ISoftDeletable"/> once the module model is complete.
/// </summary>
public sealed class SoftDeleteConvention : IModelFinalizingConvention
{
    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            entityType.SetQueryFilter(BuildFilter(entityType.ClrType));
        }
    }

    /// <summary>Applies the soft-delete query filter directly to a model builder.</summary>
    /// <param name="modelBuilder">The model builder to update.</param>
    /// <returns>The same model builder.</returns>
    public static ModelBuilder Apply(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildFilter(entityType.ClrType));
        }

        return modelBuilder;
    }

    private static LambdaExpression BuildFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var body = Expression.Equal(property, Expression.Constant(false));

        return Expression.Lambda(body, parameter);
    }
}
