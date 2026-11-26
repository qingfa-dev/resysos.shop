using BuildingBlock.Persistence.EfCore.Extensions;
using BuildingBlock.Persistence.EfCore.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlock.Persistence.PostgreSql;

/// <summary>Composition helpers for the PostgreSQL persistence provider.</summary>
public static class PostgreSqlPersistenceExtensions
{
    /// <summary>
    /// Registers a module context backed by PostgreSQL, its unit of work and the shared
    /// interceptors. Provider settings are bound from
    /// <see cref="PostgreSqlPersistenceConstant.Configuration.SectionName"/>.
    /// </summary>
    /// <typeparam name="TContext">The module context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddPersistencePostgreSql<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : ModuleDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string? fallback = configuration.GetConnectionString(
            PostgreSqlPersistenceConstant.Configuration.ConnectionName);
        IConfigurationSection section = configuration.GetSection(
            PostgreSqlPersistenceConstant.Configuration.SectionName);

        services.AddOptions<PostgreSqlPersistenceOptions>()
            .Bind(section)
            .Configure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    options.ConnectionString = fallback;
                }
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                PostgreSqlPersistenceConstant.Validation.ConnectionStringRequired)
            .Validate(
                options => options.CommandTimeoutSeconds
                    >= PostgreSqlPersistenceConstant.Constraints.CommandTimeoutSeconds.Min
                    && options.CommandTimeoutSeconds
                    <= PostgreSqlPersistenceConstant.Constraints.CommandTimeoutSeconds.Max,
                PostgreSqlPersistenceConstant.Validation.CommandTimeoutOutOfRange)
            .Validate(
                options => options.MaxRetryCount
                    >= PostgreSqlPersistenceConstant.Constraints.MaxRetryCount.Min
                    && options.MaxRetryCount
                    <= PostgreSqlPersistenceConstant.Constraints.MaxRetryCount.Max,
                PostgreSqlPersistenceConstant.Validation.MaxRetryCountOutOfRange)
            .Validate(
                options => options.MaxRetryDelay
                    >= PostgreSqlPersistenceConstant.Constraints.MaxRetryDelay.Min
                    && options.MaxRetryDelay
                    <= PostgreSqlPersistenceConstant.Constraints.MaxRetryDelay.Max,
                PostgreSqlPersistenceConstant.Validation.MaxRetryDelayOutOfRange)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.MigrationsHistoryTable),
                PostgreSqlPersistenceConstant.Validation.MigrationsHistoryTableRequired);

        services.AddModulePersistence<TContext>((provider, builder) =>
        {
            PostgreSqlPersistenceOptions options = provider
                .GetRequiredService<IOptions<PostgreSqlPersistenceOptions>>()
                .Value;

            builder.UseNpgsql(options.ConnectionString!, npgsql =>
            {
                npgsql.CommandTimeout(options.CommandTimeoutSeconds);
                npgsql.MigrationsHistoryTable(options.MigrationsHistoryTable);
                npgsql.EnableRetryOnFailure(options.MaxRetryCount, options.MaxRetryDelay, null);
            });

            builder.AddInterceptors(new SoftDeleteInterceptor());

            IDomainEventDispatcher? dispatcher = provider.GetService<IDomainEventDispatcher>();
            if (dispatcher is not null)
            {
                builder.AddInterceptors(new DomainEventDispatchInterceptor(dispatcher));
            }
        });

        return services;
    }

    /// <summary>
    /// Resolves the connection string, preferring the provider section and falling back to
    /// the named connection string.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="connectionName">
    /// The fallback connection string name; defaults to
    /// <see cref="PostgreSqlPersistenceConstant.Configuration.ConnectionName"/>.
    /// </param>
    /// <returns>The resolved connection string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither the provider section nor the named connection string is configured.
    /// </exception>
    public static string ResolveConnectionString(
        IConfiguration configuration,
        string connectionName = PostgreSqlPersistenceConstant.Configuration.ConnectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? provider = configuration[
            $"{PostgreSqlPersistenceConstant.Configuration.SectionName}:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(provider))
        {
            return provider;
        }

        string? fallback = configuration.GetConnectionString(connectionName);

        return !string.IsNullOrWhiteSpace(fallback)
            ? fallback
            : throw new InvalidOperationException(
                $"No PostgreSQL connection string is configured. Set "
                + $"{PostgreSqlPersistenceConstant.Configuration.SectionName}:ConnectionString "
                + $"or ConnectionStrings:{connectionName}.");
    }
}
