namespace BuildingBlock.Persistence.Abstractions;

/// <summary>
/// Guards for unit-of-work operations. Failures travel on the railway instead of throwing.
/// </summary>
public static class UnitOfWorkGuard
{
    #region Retries

    /// <summary>
    /// Checks a retry budget against the documented constraints.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <returns>The retry budget, or a validation error.</returns>
    public static Result<int> ValidMaxRetries(int maxRetries)
        => maxRetries < UnitOfWorkConstant.Constraints.MaxRetries.Min.Value
            ? Result.Failure<int>(UnitOfWorkConstant.Constraints.MaxRetries.Min.Error)
            : maxRetries > UnitOfWorkConstant.Constraints.MaxRetries.Max.Value
                ? Result.Failure<int>(UnitOfWorkConstant.Constraints.MaxRetries.Max.Error)
                : Result.Success(maxRetries);

    #endregion

    #region Transactions

    /// <summary>
    /// Resolves an optional transaction timeout to the default and checks it against the
    /// documented constraints.
    /// </summary>
    /// <param name="timeout">The requested timeout, or null to use the default.</param>
    /// <returns>The timeout, or a validation error.</returns>
    public static Result<TimeSpan> ValidTimeout(TimeSpan? timeout)
    {
        TimeSpan value = timeout ?? UnitOfWorkConstant.Defaults.TransactionTimeout;

        if (value < UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Value)
        {
            return Result.Failure<TimeSpan>(
                UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Error);
        }

        if (value > UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Value)
        {
            return Result.Failure<TimeSpan>(
                UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Error);
        }

        return Result.Success(value);
    }

    /// <summary>
    /// Checks that a transaction was supplied.
    /// </summary>
    /// <param name="transaction">The candidate transaction.</param>
    /// <returns>The transaction, or a validation error.</returns>
    public static Result<ITransaction> TransactionRequired(ITransaction? transaction)
        => transaction is null
            ? Result.Failure<ITransaction>(UnitOfWorkConstant.Constraints.Transaction.Required.Error)
            : Result.Success(transaction);

    /// <summary>
    /// Checks that no transaction is already active.
    /// </summary>
    /// <param name="isActive">Whether a transaction is active.</param>
    /// <returns>Unit, or a validation error.</returns>
    public static Result<Unit> TransactionNotActive(bool isActive)
        => isActive
            ? Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Transaction.Active.Error)
            : Result.Success();

    /// <summary>
    /// Checks that a transaction has not already completed.
    /// </summary>
    /// <param name="isCompleted">Whether the transaction has completed.</param>
    /// <returns>Unit, or a validation error.</returns>
    public static Result<Unit> TransactionNotCompleted(bool isCompleted)
        => isCompleted
            ? Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Transaction.Completed.Error)
            : Result.Success();

    /// <summary>
    /// Checks that a transaction has not exceeded its timeout.
    /// </summary>
    /// <param name="isExpired">Whether the transaction has expired.</param>
    /// <returns>Unit, or a validation error.</returns>
    public static Result<Unit> TransactionNotExpired(bool isExpired)
        => isExpired
            ? Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Transaction.Expired.Error)
            : Result.Success();

    #endregion

    #region State

    /// <summary>
    /// Checks that a unit of work has not been disposed.
    /// </summary>
    /// <param name="isDisposed">Whether the unit of work is disposed.</param>
    /// <returns>Unit, or a validation error.</returns>
    public static Result<Unit> NotDisposed(bool isDisposed)
        => isDisposed
            ? Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Disposed.Error)
            : Result.Success();

    /// <summary>
    /// Checks that an operation has not been cancelled.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to inspect.</param>
    /// <returns>Unit, or a validation error.</returns>
    public static Result<Unit> NotCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Cancelled.Error)
            : Result.Success();

    #endregion
}
