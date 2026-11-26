using BuildingBlock.Persistence.EfCore.Extensions;
using BuildingBlock.Persistence.EfCore.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlock.Persistence.SqlServer;

/// <summary>Composition helpers for the SQL Server persistence provider.</summary>
public static class SqlServerPersistenceExtensions
{
    /// <summary>
    /// Registers a module context backed by SQL Server, its unit of work and the shared
    /// interceptors. Provider settings are bound from
    /// <see cref="SqlServerPersistenceConstant.Configuration.SectionName"/>.
    /// </summary>
    /// <typeparam name="TContext">The module context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddPersistenceSqlServer<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : ModuleDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string? fallback = configuration.GetConnectionString(
            SqlServerPersistenceConstant.Configuration.ConnectionName);
        IConfigurationSection section = configuration.GetSection(
            SqlServerPersistenceConstant.Configuration.SectionName);

        services.AddOptions<SqlServerPersistenceOptions>()
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
                SqlServerPersistenceConstant.Validation.ConnectionStringRequired)
            .Validate(
                options => options.CommandTimeoutSeconds
                    >= SqlServerPersistenceConstant.Constraints.CommandTimeoutSeconds.Min
                    && options.CommandTimeoutSeconds
                    <= SqlServerPersistenceConstant.Constraints.CommandTimeoutSeconds.Max,
                SqlServerPersistenceConstant.Validation.CommandTimeoutOutOfRange)
            .Validate(
                options => options.MaxRetryCount
                    >= SqlServerPersistenceConstant.Constraints.MaxRetryCount.Min
                    && options.MaxRetryCount
                    <= SqlServerPersistenceConstant.Constraints.MaxRetryCount.Max,
                SqlServerPersistenceConstant.Validation.MaxRetryCountOutOfRange)
            .Validate(
                options => options.MaxRetryDelay
                    >= SqlServerPersistenceConstant.Constraints.MaxRetryDelay.Min
                    && options.MaxRetryDelay
                    <= SqlServerPersistenceConstant.Constraints.MaxRetryDelay.Max,
                SqlServerPersistenceConstant.Validation.MaxRetryDelayOutOfRange)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.MigrationsHistoryTable),
                SqlServerPersistenceConstant.Validation.MigrationsHistoryTableRequired);

        services.AddModulePersistence<TContext>((provider, builder) =>
        {
            SqlServerPersistenceOptions options = provider
                .GetRequiredService<IOptions<SqlServerPersistenceOptions>>()
                .Value;

            builder.UseSqlServer(options.ConnectionString!, sqlServer =>
            {
                sqlServer.CommandTimeout(options.CommandTimeoutSeconds);
                sqlServer.MigrationsHistoryTable(options.MigrationsHistoryTable);
                sqlServer.EnableRetryOnFailure(
                    options.MaxRetryCount,
                    options.MaxRetryDelay,
                    null);
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
    /// <see cref="SqlServerPersistenceConstant.Configuration.ConnectionName"/>.
    /// </param>
    /// <returns>The resolved connection string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither the provider section nor the named connection string is configured.
    /// </exception>
    public static string ResolveConnectionString(
        IConfiguration configuration,
        string connectionName = SqlServerPersistenceConstant.Configuration.ConnectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? provider = configuration[
            $"{SqlServerPersistenceConstant.Configuration.SectionName}:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(provider))
        {
            return provider;
        }

        string? fallback = configuration.GetConnectionString(connectionName);

        return !string.IsNullOrWhiteSpace(fallback)
            ? fallback
            : throw new InvalidOperationException(
                $"No SQL Server connection string is configured. Set "
                + $"{SqlServerPersistenceConstant.Configuration.SectionName}:ConnectionString "
                + $"or ConnectionStrings:{connectionName}.");
    }
}
