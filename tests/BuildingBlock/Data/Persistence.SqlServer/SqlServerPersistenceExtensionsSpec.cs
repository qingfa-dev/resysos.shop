using BuildingBlock.Persistence.Abstractions;
using BuildingBlock.Persistence.Core;
using BuildingBlock.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlock.Persistence.SqlServer.Unit.Tests;

/// <summary>A minimal module context used by the provider specifications.</summary>
internal sealed class TestContext : ModuleDbContext
{
    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    protected override string SchemaName => "test";

    protected override string ModuleName => "Test";
}

/// <summary>Specifications for SQL Server persistence registration and options.</summary>
public sealed class SqlServerPersistenceExtensionsSpec
{
    private const string ConnectionString =
        "Server=localhost;Database=resys;User Id=sa;Password=Password123!;TrustServerCertificate=True";

    [Fact]
    public void AddPersistenceSqlServer_Should_Register_The_Context_And_Unit_Of_Work()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:resysdb"] = ConnectionString
        });
        var services = new ServiceCollection();

        services.AddPersistenceSqlServer<TestContext>(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<TestContext>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Should().BeOfType<TestContext>();
    }

    [Fact]
    public void AddPersistenceSqlServer_Should_Bind_Provider_Settings()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BuildingBlocks:Persistence:SqlServer:ConnectionString"] = ConnectionString,
            ["BuildingBlocks:Persistence:SqlServer:CommandTimeoutSeconds"] = "17",
            ["BuildingBlocks:Persistence:SqlServer:MigrationsHistoryTable"] = "sample_history",
            ["BuildingBlocks:Persistence:SqlServer:MaxRetryCount"] = "2"
        });
        var services = new ServiceCollection();

        services.AddPersistenceSqlServer<TestContext>(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        SqlServerPersistenceOptions options = provider
            .GetRequiredService<IOptions<SqlServerPersistenceOptions>>()
            .Value;

        options.CommandTimeoutSeconds.Should().Be(17);
        options.MigrationsHistoryTable.Should().Be("sample_history");
        options.MaxRetryCount.Should().Be(2);
    }

    [Fact]
    public void AddPersistenceSqlServer_Should_Use_SqlServer()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:resysdb"] = ConnectionString
        });
        var services = new ServiceCollection();

        services.AddPersistenceSqlServer<TestContext>(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        DbContextOptions<TestContext> options = provider
            .GetRequiredService<DbContextOptions<TestContext>>();

        options.Extensions.Should().Contain(extension =>
            extension.GetType().Name.Contains("SqlServer", StringComparison.Ordinal));
    }

    [Fact]
    public void AddPersistenceSqlServer_Should_Fail_Startup_Without_A_Connection_String()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>());
        var services = new ServiceCollection();

        services.AddPersistenceSqlServer<TestContext>(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        Action act = () => _ = provider.GetRequiredService<IOptions<SqlServerPersistenceOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddPersistenceSqlServer_Should_Fail_Startup_With_An_Invalid_Retry_Delay()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:resysdb"] = ConnectionString,
            ["BuildingBlocks:Persistence:SqlServer:MaxRetryDelay"] = "00:00:00"
        });
        var services = new ServiceCollection();

        services.AddPersistenceSqlServer<TestContext>(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        Action act = () => _ = provider.GetRequiredService<IOptions<SqlServerPersistenceOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void ResolveConnectionString_Should_Prefer_The_Provider_Section()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BuildingBlocks:Persistence:SqlServer:ConnectionString"] = "Server=provider;Database=resys",
            ["ConnectionStrings:resysdb"] = "Server=fallback;Database=resys"
        });

        SqlServerPersistenceExtensions.ResolveConnectionString(configuration)
            .Should().Be("Server=provider;Database=resys");
    }

    [Fact]
    public void ResolveConnectionString_Should_Fall_Back_To_ConnectionStrings()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:resysdb"] = "Server=fallback;Database=resys"
        });

        SqlServerPersistenceExtensions.ResolveConnectionString(configuration, "resysdb")
            .Should().Be("Server=fallback;Database=resys");
    }

    [Fact]
    public void ResolveConnectionString_Should_Fail_Loudly_When_Missing()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>());

        Action act = () => SqlServerPersistenceExtensions.ResolveConnectionString(configuration);

        act.Should().Throw<InvalidOperationException>();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
