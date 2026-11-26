using BuildingBlock.Persistence.Core.Transactions;
using BuildingBlock.Persistence.EfCore.Extensions;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Persistence.Core;

/// <summary>
/// Base context for a module. It owns the module schema, applies the shared conventions
/// (optimistic concurrency and soft delete), and exposes the module as a unit of work so a
/// handler can save and transact through the railway.
/// </summary>
public abstract class ModuleDbContext : DbContext, IUnitOfWork
{
    private EfTransaction? _activeTransaction;
    private bool _disposed;

    /// <summary>Initializes a new module context.</summary>
    /// <param name="options">The provider options supplied by the composition root.</param>
    protected ModuleDbContext(DbContextOptions options)
        : base(options)
    {
    }

    #region Module Identity

    /// <summary>Gets the database schema owned by the module.</summary>
    protected abstract string SchemaName { get; }

    /// <summary>Gets the logical module name used for diagnostics.</summary>
    protected abstract string ModuleName { get; }

    /// <summary>Gets the time source used for transaction expiry.</summary>
    protected virtual TimeProvider TimeSource => TimeProvider.System;

    /// <summary>
    /// Gets the time source for infrastructure in this assembly. Keeping it internal leaves the
    /// public surface unchanged while letting interceptors share the module clock.
    /// </summary>
    internal TimeProvider InfrastructureTimeSource => TimeSource;

    #endregion

    #region Model

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        if (!ProviderIgnoresSchema())
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ConfigureOptimisticConcurrency();
        SoftDeleteConvention.Apply(modelBuilder);
    }

    private bool ProviderIgnoresSchema()
    {
        string? provider = Database.ProviderName;

        return provider is not null
            && (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
                || provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Unit Of Work

    /// <inheritdoc />
    public async Task<Result<int>> SaveChangesAsync(
        int maxRetries,
        CancellationToken cancellationToken = default)
    {
        var guard = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidMaxRetries(maxRetries));

        if (guard.IsFailure)
        {
            return Result.Failure<int>(guard.Errors);
        }

        var strategy = Database.CreateExecutionStrategy();

        for (int attempt = 0; ; attempt++)
        {
            try
            {
                int affected = await strategy
                    .ExecuteAsync(() => base.SaveChangesAsync(cancellationToken))
                    .ConfigureAwait(false);

                return Result.Success(affected);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure<int>(PersistenceConstant.Constraints.Concurrency.Error);
            }
            catch (OperationCanceledException)
            {
                return Result.Failure<int>(UnitOfWorkConstant.Constraints.Cancelled.Error);
            }
            catch (DbUpdateException) when (attempt < maxRetries && strategy.RetriesOnFailure)
            {
                await Task.Delay(PersistenceConstant.Defaults.RetryDelay, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException exception)
            {
                return Result.Failure<int>(
                    PersistenceConstant.Constraints.SaveFailed.For(exception));
            }
        }
    }

    /// <inheritdoc />
    public async Task<Result<ITransaction>> BeginTransactionAsync(
        TimeSpan? timeout,
        CancellationToken cancellationToken = default)
    {
        var guard = UnitOfWorkGuard.NotDisposed(_disposed)
            .Bind(_ => UnitOfWorkGuard.NotCancelled(cancellationToken))
            .Bind(_ => UnitOfWorkGuard.ValidTimeout(timeout))
            .Bind(valid => UnitOfWorkGuard
                .TransactionNotActive(_activeTransaction is not null)
                .Map(_ => valid));

        if (guard.IsFailure)
        {
            return Result.Failure<ITransaction>(guard.Errors);
        }

        try
        {
            if (Database.IsRelational())
            {
                Database.SetCommandTimeout(guard.Value);
            }

            var transaction = await Database.BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            var handle = new EfTransaction(
                transaction,
                guard.Value,
                TimeSource,
                CompleteTransaction);

            _activeTransaction = handle;
            return Result.Success<ITransaction>(handle);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<ITransaction>(UnitOfWorkConstant.Constraints.Cancelled.Error);
        }
        catch (Exception exception)
        {
            return Result.Failure<ITransaction>(
                PersistenceConstant.Constraints.TransactionFailed.For(exception));
        }
    }

    private void CompleteTransaction(EfTransaction transaction)
    {
        if (ReferenceEquals(_activeTransaction, transaction))
        {
            _activeTransaction = null;
        }
    }

    #endregion

    #region Lifecycle

    /// <inheritdoc />
    public override void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _activeTransaction?.Dispose();
            _activeTransaction = null;
        }

        base.Dispose();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;

            if (_activeTransaction is not null)
            {
                await _activeTransaction.DisposeAsync().ConfigureAwait(false);
            }

            _activeTransaction = null;
        }

        await base.DisposeAsync().ConfigureAwait(false);
    }

    #endregion
}
