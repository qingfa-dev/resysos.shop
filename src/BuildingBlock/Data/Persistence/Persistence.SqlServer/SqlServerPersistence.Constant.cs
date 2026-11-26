namespace BuildingBlock.Persistence.SqlServer;

/// <summary>Unified constants and error definitions for the SQL Server persistence provider.</summary>
public static partial class SqlServerPersistenceConstant
{
    #region Configuration

    /// <summary>Configuration keys read by the SQL Server provider.</summary>
    public static class Configuration
    {
        /// <summary>The configuration section that holds the provider settings.</summary>
        public const string SectionName = "BuildingBlocks:Persistence:SqlServer";

        /// <summary>The connection string name used as the fallback.</summary>
        public const string ConnectionName = "resysdb";
    }

    #endregion

    #region Constraints

    /// <summary>Constraints enforced by the SQL Server provider.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the command timeout.</summary>
        public static class CommandTimeoutSeconds
        {
            /// <summary>The smallest allowed command timeout.</summary>
            public const int Min = 0;

            /// <summary>The largest allowed command timeout.</summary>
            public const int Max = 600;
        }

        /// <summary>Constraints and error definitions for the retry budget.</summary>
        public static class MaxRetryCount
        {
            /// <summary>The smallest allowed retry budget.</summary>
            public const int Min = 0;

            /// <summary>The largest allowed retry budget.</summary>
            public const int Max = 10;
        }

        /// <summary>Constraints and error definitions for the retry delay.</summary>
        public static class MaxRetryDelay
        {
            /// <summary>The smallest allowed retry delay.</summary>
            public static readonly TimeSpan Min = TimeSpan.FromMilliseconds(1);

            /// <summary>The largest allowed retry delay.</summary>
            public static readonly TimeSpan Max = TimeSpan.FromMinutes(5);
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values applied when a caller does not specify its own.</summary>
    public static class Defaults
    {
        /// <summary>The default command timeout, in seconds.</summary>
        public const int CommandTimeoutSeconds = PersistenceConstant.Defaults.CommandTimeoutSeconds;

        /// <summary>The default migrations history table.</summary>
        public const string MigrationsHistoryTable = PersistenceConstant.Defaults.MigrationsHistoryTable;

        /// <summary>The default retry budget for transient failures.</summary>
        public const int MaxRetryCount = UnitOfWorkConstant.Defaults.MaxRetries;

        /// <summary>The default delay between retries.</summary>
        public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(5);
    }

    #endregion

    #region Validation

    /// <summary>Messages surfaced when provider options fail validation.</summary>
    public static class Validation
    {
        /// <summary>The connection string must be supplied.</summary>
        public const string ConnectionStringRequired =
            "A SQL Server connection string is required (set BuildingBlocks:Persistence:SqlServer:ConnectionString or ConnectionStrings:resysdb).";

        /// <summary>The command timeout must be within its constraints.</summary>
        public const string CommandTimeoutOutOfRange =
            "SQL Server CommandTimeoutSeconds must be between 0 and 600.";

        /// <summary>The retry budget must be within its constraints.</summary>
        public const string MaxRetryCountOutOfRange =
            "SQL Server MaxRetryCount must be between 0 and 10.";

        /// <summary>The retry delay must be within its constraints.</summary>
        public const string MaxRetryDelayOutOfRange =
            "SQL Server MaxRetryDelay must be between 1 millisecond and 5 minutes.";

        /// <summary>The migrations history table must be supplied.</summary>
        public const string MigrationsHistoryTableRequired =
            "A SQL Server migrations history table name is required.";
    }

    #endregion
}
