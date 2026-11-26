namespace BuildingBlock.Persistence.SqlServer;

/// <summary>Settings for the SQL Server persistence provider.</summary>
public sealed class SqlServerPersistenceOptions
{
    /// <summary>The configuration section that holds the provider settings.</summary>
    public const string SectionName = SqlServerPersistenceConstant.Configuration.SectionName;

    /// <summary>Gets or sets the SQL Server connection string.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the command timeout, in seconds.</summary>
    public int CommandTimeoutSeconds { get; set; } =
        SqlServerPersistenceConstant.Defaults.CommandTimeoutSeconds;

    /// <summary>Gets or sets the table that records applied migrations.</summary>
    public string MigrationsHistoryTable { get; set; } =
        SqlServerPersistenceConstant.Defaults.MigrationsHistoryTable;

    /// <summary>Gets or sets the retry budget for transient failures.</summary>
    public int MaxRetryCount { get; set; } =
        SqlServerPersistenceConstant.Defaults.MaxRetryCount;

    /// <summary>Gets or sets the delay between retries.</summary>
    public TimeSpan MaxRetryDelay { get; set; } =
        SqlServerPersistenceConstant.Defaults.MaxRetryDelay;
}
