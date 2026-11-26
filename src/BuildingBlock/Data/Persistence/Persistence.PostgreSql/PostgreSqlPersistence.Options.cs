namespace BuildingBlock.Persistence.PostgreSql;

/// <summary>Settings for the PostgreSQL persistence provider.</summary>
public sealed class PostgreSqlPersistenceOptions
{
    /// <summary>The configuration section that holds the provider settings.</summary>
    public const string SectionName = PostgreSqlPersistenceConstant.Configuration.SectionName;

    /// <summary>Gets or sets the Npgsql connection string.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the command timeout, in seconds.</summary>
    public int CommandTimeoutSeconds { get; set; } =
        PostgreSqlPersistenceConstant.Defaults.CommandTimeoutSeconds;

    /// <summary>Gets or sets the table that records applied migrations.</summary>
    public string MigrationsHistoryTable { get; set; } =
        PostgreSqlPersistenceConstant.Defaults.MigrationsHistoryTable;

    /// <summary>Gets or sets the retry budget for transient failures.</summary>
    public int MaxRetryCount { get; set; } =
        PostgreSqlPersistenceConstant.Defaults.MaxRetryCount;

    /// <summary>Gets or sets the delay between retries.</summary>
    public TimeSpan MaxRetryDelay { get; set; } =
        PostgreSqlPersistenceConstant.Defaults.MaxRetryDelay;
}
