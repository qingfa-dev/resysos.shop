namespace Testing.Integration.SqlServer;

/// <summary>SQL Server test-container settings.</summary>
public sealed class SqlServerTestOptions
{
    /// <summary>The configuration section for the fixture.</summary>
    public const string SectionName = "Testing:Integration:SqlServer";

    /// <summary>Gets or sets the container image.</summary>
    public string Image { get; set; } = "mcr.microsoft.com/mssql/server:2022-latest";

    /// <summary>Gets or sets the database name.</summary>
    public string Database { get; set; } = "resys_test";

    /// <summary>Gets or sets the system administrator password.</summary>
    /// <remarks>SQL Server enforces a complexity policy, so the default is not trivial.</remarks>
    public string Password { get; set; } = "yourStrong(!)Password";

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

        if (string.IsNullOrWhiteSpace(Password))
        {
            errors.Add("Password must not be empty.");
        }

        return errors;
    }
}
