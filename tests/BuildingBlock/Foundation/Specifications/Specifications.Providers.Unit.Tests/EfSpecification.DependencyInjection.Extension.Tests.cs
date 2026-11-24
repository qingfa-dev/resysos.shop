using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="EfSpecificationDependencyInjectionExtensions"/>.</summary>
public sealed class EfSpecificationDependencyInjectionExtensionTests
{
    #region Registration

    [Fact]
    public void AddSpecificationsEfCore_NullServices_ShouldReturnError()
    {
        IServiceCollection? services = null;

        services.AddSpecificationsEfCore().Errors.Should().ContainSingle()
            .Which.Should().Be(EfSpecificationConstant.Services.Required.Error);
    }

    [Fact]
    public void AddSpecificationsEfCore_ShouldRegisterEvaluator()
    {
        var services = new ServiceCollection();

        var result = services.AddSpecificationsEfCore();

        result.IsSuccess.Should().BeTrue();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEfSpecificationEvaluator>()
            .Should().BeOfType<EfSpecificationEvaluator>();
    }

    [Fact]
    public void AddSpecificationsEfCore_ShouldNotOverrideExistingEvaluator()
    {
        var services = new ServiceCollection();
        var evaluator = EfSpecificationEvaluator.Create(
            Options.Create(new EfSpecificationOptions())).Value;
        services.AddSingleton<IEfSpecificationEvaluator>(evaluator);

        services.AddSpecificationsEfCore();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEfSpecificationEvaluator>().Should().BeSameAs(evaluator);
    }

    #endregion

    #region Configuration

    [Fact]
    public void AddSpecificationsEfCore_ShouldBindConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{EfSpecificationOptions.SectionName}:IgnoreQueryFilters"] = "true",
                [$"{EfSpecificationOptions.SectionName}:UseSplitQuery"] = "true",
            })
            .Build();
        var services = new ServiceCollection();

        var result = services.AddSpecificationsEfCore(configuration);

        result.IsSuccess.Should().BeTrue();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<EfSpecificationOptions>>().Value;
        options.IgnoreQueryFilters.Should().BeTrue();
        options.UseSplitQuery.Should().BeTrue();
    }

    #endregion
}
