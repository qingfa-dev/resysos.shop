namespace BuildingBlock.Persistence.Abstractions;

/// <summary>Unified constants and error definitions for the unit-of-work concern.</summary>
public static partial class UnitOfWorkConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by a unit of work.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the retry budget.</summary>
        public static class MaxRetries
        {
            /// <summary>Minimum allowed retry budget.</summary>
            public static class Min
            {
                /// <summary>The smallest allowed number of retries.</summary>
                public const int Value = 0;

                /// <summary>The error returned when the retry budget is below the minimum.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.max_retries.invalid",
                    message: $"Max retries must be at least {Value}.");
            }

            /// <summary>Maximum allowed retry budget.</summary>
            public static class Max
            {
                /// <summary>The largest allowed number of retries.</summary>
                public const int Value = 10;

                /// <summary>The error returned when the retry budget exceeds the maximum.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.max_retries.invalid",
                    message: $"Max retries cannot exceed {Value}.");
            }
        }

        /// <summary>Constraints and error definitions for the transaction timeout.</summary>
        public static class TransactionTimeout
        {
            /// <summary>Minimum allowed transaction timeout.</summary>
            public static class Min
            {
                /// <summary>The shortest allowed transaction lifetime.</summary>
                public static readonly TimeSpan Value = TimeSpan.FromSeconds(1);

                /// <summary>The error returned when the timeout is below the minimum.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.timeout.invalid",
                    message: $"Transaction timeout must be at least {Value}.");
            }

            /// <summary>Maximum allowed transaction timeout.</summary>
            public static class Max
            {
                /// <summary>The longest allowed transaction lifetime.</summary>
                public static readonly TimeSpan Value = TimeSpan.FromMinutes(5);

                /// <summary>The error returned when the timeout exceeds the maximum.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.timeout.invalid",
                    message: $"Transaction timeout cannot exceed {Value}.");
            }
        }

        /// <summary>Constraints and error definitions for a missing transaction.</summary>
        public static class Transaction
        {
            /// <summary>The transaction must be present (non-null).</summary>
            public static class Required
            {
                /// <summary>The error returned when no transaction is supplied.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.transaction.required",
                    message: "A transaction is required.");
            }

            /// <summary>The transaction must not already be active.</summary>
            public static class Active
            {
                /// <summary>The error returned when a transaction is already active.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.transaction.active",
                    message: "A transaction is already active.");
            }

            /// <summary>The transaction must not already be completed.</summary>
            public static class Completed
            {
                /// <summary>The error returned when a completed transaction is reused.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.transaction.completed",
                    message: "The transaction has already completed.");
            }

            /// <summary>The transaction must not have exceeded its timeout.</summary>
            public static class Expired
            {
                /// <summary>The error returned when an expired transaction is committed.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "unit_of_work.transaction.expired",
                    message: "The transaction has expired.");
            }
        }

        /// <summary>Constraints and error definitions for a disposed unit of work.</summary>
        public static class Disposed
        {
            /// <summary>The error returned when a disposed unit of work is used.</summary>
            public static readonly Error Error = Error.Validation(
                code: "unit_of_work.disposed",
                message: "The unit of work has been disposed.");
        }

        /// <summary>Constraints and error definitions for a cancelled operation.</summary>
        public static class Cancelled
        {
            /// <summary>The error returned when an operation is cancelled.</summary>
            public static readonly Error Error = Error.Validation(
                code: "unit_of_work.cancelled",
                message: "The operation was cancelled.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values applied when a caller does not specify its own.</summary>
    public static class Defaults
    {
        /// <summary>The default number of retry attempts for transient failures.</summary>
        public const int MaxRetries = 3;

        /// <summary>The default maximum lifetime of a transaction.</summary>
        public static readonly TimeSpan TransactionTimeout = TimeSpan.FromSeconds(30);
    }

    #endregion
}
