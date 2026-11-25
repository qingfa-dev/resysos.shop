using System.Collections.Concurrent;

using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Persistence.InMemory;

/// <summary>
/// Result-only in-memory implementation of <see cref="IInMemoryRepository{TAggregate, TId}"/>.
/// Reads take a predicate, which is compiled and applied to the stored aggregates.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class InMemoryRepository<TAggregate, TId> : IInMemoryRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TAggregate> _items = new();

    #region Construction

    /// <summary>
    /// Protected so instances can only be created through <see cref="Create()"/>.
    /// </summary>
    protected InMemoryRepository()
    {
    }

    #endregion

    #region Factory

    /// <summary>Creates an empty in-memory repository.</summary>
    /// <returns>The repository.</returns>
    public static Result<InMemoryRepository<TAggregate, TId>> Create()
        => Result.Success(new InMemoryRepository<TAggregate, TId>());

    #endregion

    #region Lookups

    /// <inheritdoc />
    public Task<Result<TAggregate>> FindAsync(TId id, CancellationToken cancellationToken = default)
        => Task.FromResult(
            RepositoryGuard.ValidId(id)
                .Bind(validId => _items.TryGetValue(validId, out TAggregate? aggregate)
                    ? Result.Success(aggregate)
                    : Result.Failure<TAggregate>(
                        InMemoryRepositoryConstant.Constraints.NotFound.For<TAggregate, TId>(validId))));

    #endregion

    #region Writes

    /// <inheritdoc />
    public Task<Result<TAggregate>> AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(AddCore(aggregate, overwrite: false));

    /// <inheritdoc />
    public Task<Result<TAggregate>> UpdateAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(UpdateCore(aggregate));

    /// <inheritdoc />
    public Task<Result<TAggregate>> RemoveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(RemoveCore(aggregate));

    /// <inheritdoc />
    public Result<TAggregate> Seed(TAggregate aggregate) => AddCore(aggregate, overwrite: true);

    /// <inheritdoc />
    public Result<Unit> Clear()
    {
        _items.Clear();
        return Result.Success();
    }

    #endregion

    #region Queries

    /// <inheritdoc />
    public Task<Result<bool>> ExistsAsync(TId id, CancellationToken cancellationToken = default)
        => Task.FromResult(
            RepositoryGuard.ValidId(id)
                .Map(validId => _items.ContainsKey(validId)));

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<TAggregate>>> ListAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(
            RepositoryGuard.PredicateRequired(predicate)
                .Map(valid => (IReadOnlyList<TAggregate>)_items.Values
                    .Where(valid.Compile())
                    .ToList()));

    /// <inheritdoc />
    public async Task<Result<TAggregate>> FirstOrDefaultAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var list = await ListAsync(predicate, cancellationToken).ConfigureAwait(false);

        return list.Bind(items => items.Count == 0
            ? Result.Failure<TAggregate>(
                InMemoryRepositoryConstant.Constraints.NotFound.NoMatch.Error)
            : Result.Success(items[0]));
    }

    /// <inheritdoc />
    public async Task<Result<int>> CountAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var list = await ListAsync(predicate, cancellationToken).ConfigureAwait(false);

        return list.Map(items => items.Count);
    }

    #endregion

    #region Helpers

    private Result<TAggregate> AddCore(TAggregate aggregate, bool overwrite)
    {
        var valid = RepositoryGuard.NotNull(aggregate);
        if (valid.IsFailure)
        {
            return Result.Failure<TAggregate>(valid.Errors);
        }

        var validId = RepositoryGuard.ValidId(valid.Value.Id);
        if (validId.IsFailure)
        {
            return Result.Failure<TAggregate>(validId.Errors);
        }

        if (overwrite)
        {
            _items[validId.Value] = valid.Value;
            return Result.Success(valid.Value);
        }

        return _items.TryAdd(validId.Value, valid.Value)
            ? Result.Success(valid.Value)
            : Result.Failure<TAggregate>(
                InMemoryRepositoryConstant.Constraints.Conflict.AlreadyExists<TAggregate, TId>(validId.Value));
    }

    private Result<TAggregate> UpdateCore(TAggregate aggregate)
    {
        var valid = RepositoryGuard.NotNull(aggregate);
        if (valid.IsFailure)
        {
            return Result.Failure<TAggregate>(valid.Errors);
        }

        var validId = RepositoryGuard.ValidId(valid.Value.Id);
        if (validId.IsFailure)
        {
            return Result.Failure<TAggregate>(validId.Errors);
        }

        if (!_items.ContainsKey(validId.Value))
        {
            return Result.Failure<TAggregate>(
                InMemoryRepositoryConstant.Constraints.NotFound.For<TAggregate, TId>(validId.Value));
        }

        _items[validId.Value] = valid.Value;
        return Result.Success(valid.Value);
    }

    private Result<TAggregate> RemoveCore(TAggregate aggregate)
    {
        var valid = RepositoryGuard.NotNull(aggregate);
        if (valid.IsFailure)
        {
            return Result.Failure<TAggregate>(valid.Errors);
        }

        var validId = RepositoryGuard.ValidId(valid.Value.Id);
        if (validId.IsFailure)
        {
            return Result.Failure<TAggregate>(validId.Errors);
        }

        return _items.TryRemove(validId.Value, out TAggregate? removed)
            ? Result.Success(removed)
            : Result.Failure<TAggregate>(
                InMemoryRepositoryConstant.Constraints.NotFound.For<TAggregate, TId>(validId.Value));
    }

    #endregion
}
