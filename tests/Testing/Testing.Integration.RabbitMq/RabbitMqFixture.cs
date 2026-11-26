using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Testing.Integration.Containers;

namespace Testing.Integration.RabbitMq;

/// <summary>Owns a throwaway RabbitMQ instance for integration and end-to-end tests.</summary>
/// <remarks>
/// Works with Docker and Podman through <see cref="ContainerRuntime"/>. Readiness is checked on the
/// published host port, which avoids container <c>exec</c> calls that rootless Podman can deny.
/// </remarks>
public sealed class RabbitMqFixture : IAsyncDisposable
{
    private const int AmqpPort = 5672;
    private const string Username = "guest";
    private const string Password = "guest";

    static RabbitMqFixture() => ContainerRuntime.Configure();

    private readonly IContainer _container;

    /// <summary>Initializes the fixture.</summary>
    /// <param name="options">Container settings, or <see langword="null"/> for the defaults.</param>
    public RabbitMqFixture(RabbitMqTestOptions? options = null)
    {
        string image = (options ?? new RabbitMqTestOptions()).Image;
        _container = new ContainerBuilder(image)
            .WithPortBinding(AmqpPort, assignRandomHostPort: true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", Username)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(AmqpPort))
            .Build();
    }

    /// <summary>Gets the connection string of the running container.</summary>
    public string ConnectionString =>
        $"amqp://{Username}:{Password}@{_container.Hostname}:{_container.GetMappedPublicPort(AmqpPort)}/";

    /// <summary>Starts the container and waits until the broker accepts connections.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _container.StartAsync(cancellationToken);

        var factory = new ConnectionFactory { Uri = new Uri(ConnectionString) };
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (true)
        {
            try
            {
                await using IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
                return;
            }
#pragma warning disable CA1031 // The broker is still booting; retry until the deadline.
            catch (Exception) when (DateTimeOffset.UtcNow < deadline)
#pragma warning restore CA1031
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

/// <summary>Registration for the RabbitMQ integration fixture.</summary>
public static class RabbitMqFixtureExtensions
{
    /// <summary>Registers the fixture and its container settings.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional settings overrides.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddRabbitMqTestFixture(
        this IServiceCollection services,
        Action<RabbitMqTestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new RabbitMqTestOptions();
        configure?.Invoke(options);
        IReadOnlyList<string> errors = options.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The RabbitMQ fixture settings are invalid: " + string.Join(" ", errors));
        }

        services.AddSingleton(options);
        services.AddSingleton(provider => new RabbitMqFixture(
            provider.GetRequiredService<RabbitMqTestOptions>()));
        return services;
    }
}
