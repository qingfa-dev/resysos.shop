using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;


namespace BuildingBlock.Specifications.EfCore;

/// <summary>Registration entry point for the EF Core specification provider.</summary>
public static class EfSpecificationDependencyInjectionExtensions
{
    #region Registration

    /// <summary>Registers the EF Core evaluator and its settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration to bind the settings from.</param>
    /// <returns>The same service collection, or validation errors.</returns>
    public static Result<IServiceCollection> AddSpecificationsEfCore(
        this IServiceCollection? services,
        IConfiguration? configuration = null)
    {
        var validServices = EfSpecificationGuard.ServicesRequired(services);
        if (validServices.IsFailure)
            return Result.Failure<IServiceCollection>(validServices.Errors);

        if (configuration is not null)
        {
            validServices.Value.Configure<EfSpecificationOptions>(
                configuration.GetSection(EfSpecificationOptions.SectionName));
        }

        var options = new EfSpecificationOptions();
        configuration?.GetSection(EfSpecificationOptions.SectionName).Bind(options);

        var evaluator = EfSpecificationEvaluator.Create(Options.Create(options));
        if (evaluator.IsFailure)
            return Result.Failure<IServiceCollection>(evaluator.Errors);

        validServices.Value.TryAddSingleton<IEfSpecificationEvaluator>(evaluator.Value);
        return Result.Success(validServices.Value);
    }

    #endregion
}
