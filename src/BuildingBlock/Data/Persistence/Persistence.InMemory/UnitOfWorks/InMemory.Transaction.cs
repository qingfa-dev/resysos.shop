namespace BuildingBlock.Persistence.InMemory.UnitOfWorks;

/// <summary>
/// In-memory transaction handle. It does not isolate writes because the in-memory store is
/// immediate, but it enforces the transaction lifecycle: a single completion, a maximum lifetime,
/// cancellation and disposal.
/// </summary>
public sealed class InMemoryTransaction : ITransaction
{
    private readonly TimeSpan _timeout;
    private readonly TimeProvider _timeProvider;
    private readonly DateTimeOffset _startedAt;
    private readonly Action<InMemoryTransaction> _onCompleted;
    private bool _completed;
    private bool _disposed;

    #region Construction

    internal InMemoryTransaction(
        TimeSpan timeout,
        TimeProvider timeProvider,
        Action<InMemoryTransaction> onCompleted)
    {
        _timeout = timeout;
        _timeProvider = timeProvider;
        _startedAt = timeProvider.GetUtcNow();
        _onCompleted = onCompleted;
    }

    #endregion

    #region State

    /// <summary>Gets a value indicating whether the transaction has completed.</summary>
    public bool IsCompleted => _completed;

    /// <summary>Gets a value indicating whether the transaction has exceeded its timeout.</summary>
    public bool IsExpired => _timeProvider.GetUtcNow() - _startedAt >= _timeout;

    #endregion

    #region Completion

    /// <inheritdoc />
    public Task<Result<Unit>> CommitAsync(
        int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
        CancellationToken cancellationToken = default)
    {
        var result = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidMaxRetries(maxRetries))
            .Bind(_ => UnitOfWorkGuard.TransactionNotCompleted(_completed))
            .Bind(_ => UnitOfWorkGuard.TransactionNotExpired(IsExpired))
            .Tap(_ => Complete());

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<Result<Unit>> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.TransactionNotCompleted(_completed))
            .Tap(_ => Complete());

        return Task.FromResult(result);
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
        Complete();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private void Complete()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _onCompleted(this);
    }

    #endregion
}
