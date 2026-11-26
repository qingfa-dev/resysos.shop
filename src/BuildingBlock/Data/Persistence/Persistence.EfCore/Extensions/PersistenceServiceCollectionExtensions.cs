using BuildingBlock.Persistence.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Persistence.EfCore.Extensions;

/// <summary>Composition helpers for a module's persistence.</summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers a module's <see cref="ModuleDbContext"/> and its unit of work. The
    /// caller supplies the provider options so the persistence block stays
    /// provider-agnostic.
    /// </summary>
    /// <typeparam name="TContext">The module context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configures the provider and connection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddModulePersistence<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
        where TContext : ModuleDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddDbContext<TContext>((provider, options) => configure(provider, options));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TContext>());
        return services;
    }
}
