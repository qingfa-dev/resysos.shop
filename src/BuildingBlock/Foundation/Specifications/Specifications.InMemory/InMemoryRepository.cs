using System.Collections.Concurrent;

namespace BuildingBlock.Specifications.InMemory;

/// <summary>Result-only in-memory implementation of <see cref="IInMemoryRepository{TAggregate, TId}"/>.</summary>
public class InMemoryRepository<TAggregate, TId> : IInMemoryRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TAggregate> _items = new();
    private readonly IInMemorySpecificationEvaluator _evaluator;

    #region Construction

    /// <summary>
    /// Protected so instances can only be created through <see cref="Create()"/>.
    /// </summary>
    protected InMemoryRepository(IInMemorySpecificationEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    #endregion

    #region Factory

    /// <summary>Creates a repository with the default evaluator.</summary>
    public static Result<InMemoryRepository<TAggregate, TId>> Create()
    {
        var evaluator = InMemorySpecificationEvaluator.Create();
        if (evaluator.IsFailure)
            return Result.Failure<InMemoryRepository<TAggregate, TId>>(evaluator.Errors);

        return Result.Success(new InMemoryRepository<TAggregate, TId>(evaluator.Value));
    }

    /// <summary>Creates a repository with a supplied evaluator.</summary>
    public static Result<InMemoryRepository<TAggregate, TId>> Create(
        IInMemorySpecificationEvaluator? evaluator)
        => InMemorySpecificationGuard.EvaluatorRequired(evaluator)
            .Map(valid => new InMemoryRepository<TAggregate, TId>(valid));

    #endregion

    #region Lookups

    /// <inheritdoc />
    public Task<Result<TAggregate>> FindAsync(TId id, CancellationToken cancellationToken = default)
        => Task.FromResult(
            InMemorySpecificationGuard.IdRequired(id)
                .Bind(validId => _items.TryGetValue(validId, out TAggregate? aggregate)
                    ? Result.Success(aggregate)
                    : Result.Failure<TAggregate>(
                        InMemorySpecificationConstant.NotFound.For<TAggregate, TId>(validId))));

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
            InMemorySpecificationGuard.IdRequired(id)
                .Map(validId => _items.ContainsKey(validId)));

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<TAggregate>>> ListAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_evaluator
            .Apply(_items.Values, specification)
            .Map(items => (IReadOnlyList<TAggregate>)items.ToList()));

    /// <inheritdoc />
    public Task<Result<TAggregate>> FirstOrDefaultAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
        => Task.FromResult(ListAsync(specification, cancellationToken).Result
            .Bind(list => list.Count == 0
                ? Result.Failure<TAggregate>(
                    InMemorySpecificationConstant.NotFound.NoMatch.Error)
                : Result.Success(list[0])));

    /// <inheritdoc />
    public Task<Result<int>> CountAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
        => Task.FromResult(ListAsync(specification, cancellationToken).Result.Map(list => list.Count));

    #endregion

    #region Helpers

    private Result<TAggregate> AddCore(TAggregate aggregate, bool overwrite)
    {
        var valid = InMemorySpecificationGuard.AggregateRequired(aggregate);
        if (valid.IsFailure)
            return Result.Failure<TAggregate>(valid.Errors);

        var validId = InMemorySpecificationGuard.IdRequired(valid.Value.Id);
        if (validId.IsFailure)
            return Result.Failure<TAggregate>(validId.Errors);

        if (overwrite)
        {
            _items[validId.Value] = valid.Value;
            return Result.Success(valid.Value);
        }

        return _items.TryAdd(validId.Value, valid.Value)
            ? Result.Success(valid.Value)
            : Result.Failure<TAggregate>(
                InMemorySpecificationConstant.Conflict.AlreadyExists<TAggregate, TId>(validId.Value));
    }

    private Result<TAggregate> UpdateCore(TAggregate aggregate)
    {
        var valid = InMemorySpecificationGuard.AggregateRequired(aggregate);
        if (valid.IsFailure)
            return Result.Failure<TAggregate>(valid.Errors);

        var validId = InMemorySpecificationGuard.IdRequired(valid.Value.Id);
        if (validId.IsFailure)
            return Result.Failure<TAggregate>(validId.Errors);

        if (!_items.ContainsKey(validId.Value))
        {
            return Result.Failure<TAggregate>(
                InMemorySpecificationConstant.NotFound.For<TAggregate, TId>(validId.Value));
        }

        _items[validId.Value] = valid.Value;
        return Result.Success(valid.Value);
    }

    private Result<TAggregate> RemoveCore(TAggregate aggregate)
    {
        var valid = InMemorySpecificationGuard.AggregateRequired(aggregate);
        if (valid.IsFailure)
            return Result.Failure<TAggregate>(valid.Errors);

        var validId = InMemorySpecificationGuard.IdRequired(valid.Value.Id);
        if (validId.IsFailure)
            return Result.Failure<TAggregate>(validId.Errors);

        return _items.TryRemove(validId.Value, out TAggregate? removed)
            ? Result.Success(removed)
            : Result.Failure<TAggregate>(
                InMemorySpecificationConstant.NotFound.For<TAggregate, TId>(validId.Value));
    }

    #endregion
}
