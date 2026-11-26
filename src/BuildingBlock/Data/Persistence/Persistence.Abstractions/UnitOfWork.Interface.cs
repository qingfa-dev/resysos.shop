namespace BuildingBlock.Persistence.Abstractions;

/// <summary>
/// Unit-of-work contract used by module handlers to commit one aggregate change in one
/// transaction. Every operation stays on the railway: no nullable or throwing variants are exposed.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Saves all pending changes to the underlying data store.
    /// </summary>
    /// <param name="maxRetries">
    /// Maximum number of retry attempts for transient failures. Defaults to
    /// <see cref="UnitOfWorkConstant.Defaults.MaxRetries"/> and must stay within
    /// <see cref="UnitOfWorkConstant.Constraints.MaxRetries"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected entities, or a failure result.</returns>
    Task<Result<int>> SaveChangesAsync(
        int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction scope.
    /// </summary>
    /// <param name="timeout">
    /// Maximum lifetime of the transaction. When omitted, the default from
    /// <see cref="UnitOfWorkConstant.Defaults.TransactionTimeout"/> is used and must stay within
    /// <see cref="UnitOfWorkConstant.Constraints.TransactionTimeout"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A transaction handle, or a failure result.</returns>
    Task<Result<ITransaction>> BeginTransactionAsync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a transaction scope. Every operation stays on the railway: no throwing variants
/// are exposed.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <param name="maxRetries">
    /// Maximum number of retry attempts for transient failures. Defaults to
    /// <see cref="UnitOfWorkConstant.Defaults.MaxRetries"/> and must stay within
    /// <see cref="UnitOfWorkConstant.Constraints.MaxRetries"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unit on success, or a failure result.</returns>
    Task<Result<Unit>> CommitAsync(
        int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unit on success, or a failure result.</returns>
    Task<Result<Unit>> RollbackAsync(CancellationToken cancellationToken = default);
}
