namespace Testing.Integration.PostgreSql;

/// <summary>PostgreSQL test-container settings.</summary>
public sealed class PostgreSqlTestOptions
{
    /// <summary>The configuration section for the fixture.</summary>
    public const string SectionName = "Testing:Integration:PostgreSql";

    /// <summary>Gets or sets the container image.</summary>
    public string Image { get; set; } = "docker.io/library/postgres:17-alpine";

    /// <summary>Gets or sets the database name.</summary>
    public string Database { get; set; } = "resys_test";

    /// <summary>Gets or sets the user name.</summary>
    public string Username { get; set; } = "postgres";

    /// <summary>Gets or sets the password.</summary>
    public string Password { get; set; } = "postgres";

    /// <summary>Validates the settings.</summary>
    /// <returns>The validation errors; empty when the settings are valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(Image))
        {
            errors.Add("Image must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(Database))
        {
            errors.Add("Database must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            errors.Add("Username must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            errors.Add("Password must not be empty.");
        }

        return errors;
    }
}