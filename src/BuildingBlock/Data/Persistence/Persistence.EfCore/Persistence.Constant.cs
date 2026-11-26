namespace BuildingBlock.Persistence.Core;

/// <summary>Unified constants and error definitions for the EF Core persistence concern.</summary>
public static partial class PersistenceConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by the EF Core persistence layer.</summary>
    public static class Constraints
    {
        /// <summary>Errors raised when saving changes fails.</summary>
        public static class SaveFailed
        {
            /// <summary>The error returned when a save fails for an unspecified reason.</summary>
            public static readonly Error Error = Error.Dependency(
                code: "persistence.save_failed",
                message: "Saving changes failed.");

            /// <summary>Creates a save error describing the failure type.</summary>
            /// <param name="exception">The failure that stopped the save.</param>
            /// <returns>The save error.</returns>
            public static Error For(Exception exception)
                => Error.Dependency(
                    code: "persistence.save_failed",
                    message: $"Saving changes failed: {exception.GetType().Name}.");
        }

        /// <summary>Errors raised when a concurrency token no longer matches.</summary>
        public static class Concurrency
        {
            /// <summary>The error returned when an update loses an optimistic concurrency race.</summary>
            public static readonly Error Error = Error.Conflict(
                code: "persistence.concurrency_conflict",
                message: "The entity was modified by another process; reload it and retry.");
        }

        /// <summary>Errors raised when a transaction cannot be started.</summary>
        public static class TransactionFailed
        {
            /// <summary>The error returned when a transaction cannot be started.</summary>
            public static readonly Error Error = Error.Dependency(
                code: "persistence.transaction_failed",
                message: "The transaction could not be started.");

            /// <summary>Creates a transaction error describing the failure type.</summary>
            /// <param name="exception">The failure that stopped the transaction.</param>
            /// <returns>The transaction error.</returns>
            public static Error For(Exception exception)
                => Error.Dependency(
                    code: "persistence.transaction_failed",
                    message: $"The transaction could not be started: {exception.GetType().Name}.");
        }

        /// <summary>Errors raised when a transaction commit fails.</summary>
        public static class CommitFailed
        {
            /// <summary>The error returned when a commit fails.</summary>
            public static readonly Error Error = Error.Dependency(
                code: "persistence.commit_failed",
                message: "The transaction could not be committed.");

            /// <summary>Creates a commit error describing the failure type.</summary>
            /// <param name="exception">The failure that stopped the commit.</param>
            /// <returns>The commit error.</returns>
            public static Error For(Exception exception)
                => Error.Dependency(
                    code: "persistence.commit_failed",
                    message: $"The transaction could not be committed: {exception.GetType().Name}.");
        }

        /// <summary>Errors raised when a transaction rollback fails.</summary>
        public static class RollbackFailed
        {
            /// <summary>The error returned when a rollback fails.</summary>
            public static readonly Error Error = Error.Dependency(
                code: "persistence.rollback_failed",
                message: "The transaction could not be rolled back.");

            /// <summary>Creates a rollback error describing the failure type.</summary>
            /// <param name="exception">The failure that stopped the rollback.</param>
            /// <returns>The rollback error.</returns>
            public static Error For(Exception exception)
                => Error.Dependency(
                    code: "persistence.rollback_failed",
                    message: $"The transaction could not be rolled back: {exception.GetType().Name}.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the EF Core persistence concern.</summary>
    public static class Defaults
    {
        /// <summary>The table that records applied migrations.</summary>
        public const string MigrationsHistoryTable = "__ef_migrations_history";

        /// <summary>The default command timeout, in seconds.</summary>
        public const int CommandTimeoutSeconds = 30;

        /// <summary>The delay applied between save retries.</summary>
        public static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(50);
    }

    #endregion
}
