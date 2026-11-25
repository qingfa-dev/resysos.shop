using BuildingBlock.Clock.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlock.Clock.System;

/// <summary>Registration helpers for system services.</summary>
public static class SystemExtensions
{
    /// <summary>
    /// Registers the clock, identifier generator, and their <see cref="TimeProvider"/>.
    /// Existing registrations win, so tests can replace them before or after this call.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddClockSystem(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<IIdGenerator, GuidIdGenerator>();
        return services;
    }
}
