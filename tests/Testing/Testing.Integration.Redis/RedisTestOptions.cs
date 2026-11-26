namespace Testing.Integration.Redis;

/// <summary>Redis test-container settings.</summary>
public sealed class RedisTestOptions
{
    /// <summary>The configuration section for the fixture.</summary>
    public const string SectionName = "Testing:Integration:Redis";

    /// <summary>Gets or sets the container image.</summary>
    public string Image { get; set; } = "redis:8.10.3-alpine";

    /// <summary>Validates the settings.</summary>
    /// <returns>The validation errors; empty when the settings are valid.</returns>
    public IReadOnlyList<string> Validate()
        => string.IsNullOrWhiteSpace(Image) ? ["Image must not be empty."] : [];
}
