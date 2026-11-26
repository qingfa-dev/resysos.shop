using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlock.Persistence.Core.Transactions;

/// <summary>
/// EF Core transaction handle. It enforces the transaction lifecycle: a single completion, a
/// maximum lifetime, cancellation and disposal.
/// </summary>
public sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly TimeSpan _timeout;
    private readonly TimeProvider _timeProvider;
    private readonly DateTimeOffset _startedAt;
    private readonly Action<EfTransaction> _onCompleted;
    private bool _completed;
    private bool _disposed;

    #region Construction

    internal EfTransaction(
        IDbContextTransaction transaction,
        TimeSpan timeout,
        TimeProvider timeProvider,
        Action<EfTransaction> onCompleted)
    {
        _transaction = transaction;
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
    public async Task<Result<Unit>> CommitAsync(
        int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
        CancellationToken cancellationToken = default)
    {
        var guard = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidMaxRetries(maxRetries))
            .Bind(_ => UnitOfWorkGuard.TransactionNotCompleted(_completed))
            .Bind(_ => UnitOfWorkGuard.TransactionNotExpired(IsExpired));

        if (guard.IsFailure)
        {
            return Result.Failure<Unit>(guard.Errors);
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            Complete();
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Cancelled.Error);
        }
        catch (Exception exception)
        {
            return Result.Failure<Unit>(
                PersistenceConstant.Constraints.CommitFailed.For(exception));
        }
    }

    /// <inheritdoc />
    public async Task<Result<Unit>> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var guard = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.TransactionNotCompleted(_completed));

        if (guard.IsFailure)
        {
            return Result.Failure<Unit>(guard.Errors);
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Complete();
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Unit>(UnitOfWorkConstant.Constraints.Cancelled.Error);
        }
        catch (Exception exception)
        {
            return Result.Failure<Unit>(
                PersistenceConstant.Constraints.RollbackFailed.For(exception));
        }
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
        _transaction.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Complete();
        await _transaction.DisposeAsync().ConfigureAwait(false);
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
