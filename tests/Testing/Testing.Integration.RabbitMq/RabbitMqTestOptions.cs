namespace Testing.Integration.RabbitMq;

/// <summary>RabbitMQ test-container settings.</summary>
public sealed class RabbitMqTestOptions
{
    /// <summary>The configuration section for the fixture.</summary>
    public const string SectionName = "Testing:Integration:RabbitMq";

    /// <summary>Gets or sets the container image.</summary>
    public string Image { get; set; } = "rabbitmq:4.4.1-management-alpine";

    /// <summary>Validates the settings.</summary>
    /// <returns>The validation errors; empty when the settings are valid.</returns>
    public IReadOnlyList<string> Validate()
        => string.IsNullOrWhiteSpace(Image) ? ["Image must not be empty."] : [];
}
