using BuildingBlock.Persistence.EfCore.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Persistence.EfCore.Unit.Tests;

/// <summary>Tests for <see cref="PersistenceServiceCollectionExtensions"/>.</summary>
public sealed class PersistenceServiceCollectionTests
{
    #region Registration

    [Fact]
    public void AddModulePersistence_ShouldRegisterContextAndUnitOfWork()
    {
        var services = new ServiceCollection();

        services.AddModulePersistence<TestContext>(
            (_, options) => options.UseSqlite("DataSource=:memory:"));

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<TestContext>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().BeOfType<TestContext>();
    }

    [Fact]
    public void AddModulePersistence_ShouldInvokeTheConfigureCallback()
    {
        var services = new ServiceCollection();
        var configured = false;

        services.AddModulePersistence<TestContext>((_, options) =>
        {
            configured = true;
            options.UseSqlite("DataSource=:memory:");
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<TestContext>();

        configured.Should().BeTrue();
    }

    [Fact]
    public void AddModulePersistence_ShouldGuardNullArguments()
    {
        var services = new ServiceCollection();

        Action nullServices = () => PersistenceServiceCollectionExtensions
            .AddModulePersistence<TestContext>(null!, (_, _) => { });
        Action nullConfigure = () => services
            .AddModulePersistence<TestContext>(null!);

        nullServices.Should().Throw<ArgumentNullException>();
        nullConfigure.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
