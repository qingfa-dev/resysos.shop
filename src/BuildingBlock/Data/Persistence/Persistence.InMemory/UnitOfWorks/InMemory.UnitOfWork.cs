namespace BuildingBlock.Persistence.InMemory.UnitOfWorks;

/// <summary>
/// In-memory implementation of <see cref="IUnitOfWork"/>. The in-memory store applies writes
/// immediately, so <see cref="SaveChangesAsync"/> has no pending work to flush and reports zero
/// affected entities. The unit of work still enforces the lifecycle rules: it validates retry
/// budgets, timeouts and cancellation, and allows at most one active transaction at a time.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly TimeProvider _timeProvider;
    private InMemoryTransaction? _activeTransaction;
    private bool _disposed;

    #region Construction

    private InMemoryUnitOfWork(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    #endregion

    #region Factory

    /// <summary>Creates an in-memory unit of work.</summary>
    /// <param name="timeProvider">
    /// The time source used for transaction expiry; defaults to the system clock.
    /// </param>
    /// <returns>The unit of work.</returns>
    public static Result<InMemoryUnitOfWork> Create(TimeProvider? timeProvider = null)
        => Result.Success(new InMemoryUnitOfWork(timeProvider ?? TimeProvider.System));

    #endregion

    #region Save

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(
        int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
        CancellationToken cancellationToken = default)
    {
        var result = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidMaxRetries(maxRetries))
            .Bind(_ => UnitOfWorkGuard.TransactionNotExpired(_activeTransaction?.IsExpired ?? false))
            .Map(_ => 0);

        return Task.FromResult(result);
    }

    #endregion

    #region Transactions

    /// <inheritdoc />
    public Task<Result<ITransaction>> BeginTransactionAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var valid = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidTimeout(timeout))
            .Bind(validTimeout => UnitOfWorkGuard
                .TransactionNotActive(_activeTransaction is not null)
                .Map(_ => validTimeout));

        if (valid.IsFailure)
        {
            return Task.FromResult(Result.Failure<ITransaction>(valid.Errors));
        }

        var transaction = new InMemoryTransaction(valid.Value, _timeProvider, Complete);
        _activeTransaction = transaction;

        return Task.FromResult(Result.Success<ITransaction>(transaction));
    }

    #endregion

    #region Lifecycle

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        InMemoryTransaction? transaction = _activeTransaction;
        _activeTransaction = null;
        transaction?.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        InMemoryTransaction? transaction = _activeTransaction;
        _activeTransaction = null;

        if (transaction is not null)
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void Complete(InMemoryTransaction transaction)
    {
        if (ReferenceEquals(_activeTransaction, transaction))
        {
            _activeTransaction = null;
        }
    }

    #endregion
}
