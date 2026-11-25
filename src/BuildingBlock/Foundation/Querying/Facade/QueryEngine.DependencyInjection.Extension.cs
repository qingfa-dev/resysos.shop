using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlock.Querying.Facade;

/// <summary>Registration entry point for the dynamic query engine.</summary>
public static class QueryEngineDependencyInjectionExtensions
{
    #region Registration

    /// <summary>
    /// Registers the dynamic query policy and engine. The provider executor
    /// (<see cref="IQueryPlanExecutor"/>) must be registered separately by the host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// An optional callback that configures the policy, typically by registering fields on its
    /// <see cref="QueryPolicy.Registry"/>.
    /// </param>
    /// <returns>The same service collection, or a validation error when it is missing.</returns>
    public static Result<IServiceCollection> AddDynamicQuery(
        this IServiceCollection? services,
        Action<QueryPolicy>? configure = null)
    {
        if (services is null)
        {
            return Result.Failure<IServiceCollection>(QueryEngineConstant.Constraints.Services.Required.Error);
        }

        var policy = new QueryPolicy();
        configure?.Invoke(policy);

        services.TryAddSingleton(policy);
        services.TryAddSingleton<IQueryEngine>(provider => provider.GetRequiredService<QueryEngine>());
        services.TryAddSingleton(provider => QueryEngine.Create(
            provider.GetRequiredService<QueryPolicy>(),
            provider.GetRequiredService<IQueryPlanExecutor>()).Value);

        return Result.Success(services);
    }

    #endregion
}
