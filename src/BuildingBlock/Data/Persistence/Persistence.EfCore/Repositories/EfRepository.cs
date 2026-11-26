using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Persistence.Core.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepository{TAggregate, TId}"/>. Reads are no-tracking;
/// writes attach the aggregate and leave the commit to the unit of work.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class EfRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    private readonly DbSet<TAggregate> _dbSet;

    /// <summary>Initializes a new repository over the supplied context.</summary>
    /// <param name="context">The module context that owns the aggregate set.</param>
    public EfRepository(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _dbSet = context.Set<TAggregate>();
    }

    #region Lookups

    /// <inheritdoc />
    public async Task<Result<TAggregate>> FindAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        var valid = RepositoryGuard.ValidId(id);
        if (valid.IsFailure)
        {
            return Result.Failure<TAggregate>(valid.Errors);
        }

        TAggregate? aggregate = await _dbSet
            .FindAsync([valid.Value], cancellationToken)
            .ConfigureAwait(false);

        return RepositoryGuard.EnsureFound(aggregate, valid.Value);
    }

    #endregion

    #region Writes

    /// <inheritdoc />
    public async Task<Result<TAggregate>> AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
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

        bool exists = await _dbSet
            .AsNoTracking()
            .AnyAsync(ById(validId.Value), cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            return Result.Failure<TAggregate>(
                RepositoryConstant.Constraints.Conflict
                    .AlreadyExists<TAggregate, TId>(validId.Value));
        }

        await _dbSet.AddAsync(valid.Value, cancellationToken).ConfigureAwait(false);
        return Result.Success(valid.Value);
    }

    /// <inheritdoc />
    public async Task<Result<TAggregate>> UpdateAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
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

        bool exists = await _dbSet
            .AsNoTracking()
            .AnyAsync(ById(validId.Value), cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure<TAggregate>(
                RepositoryConstant.Constraints.NotFound
                    .For<TAggregate, TId>(validId.Value));
        }

        _dbSet.Update(valid.Value);
        return Result.Success(valid.Value);
    }

    /// <inheritdoc />
    public async Task<Result<TAggregate>> RemoveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
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

        bool exists = await _dbSet
            .AsNoTracking()
            .AnyAsync(ById(validId.Value), cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure<TAggregate>(
                RepositoryConstant.Constraints.NotFound
                    .For<TAggregate, TId>(validId.Value));
        }

        _dbSet.Remove(valid.Value);
        return Result.Success(valid.Value);
    }

    #endregion

    #region Queries

    /// <inheritdoc />
    public async Task<Result<bool>> ExistsAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        var valid = RepositoryGuard.ValidId(id);
        if (valid.IsFailure)
        {
            return Result.Failure<bool>(valid.Errors);
        }

        bool exists = await _dbSet
            .AsNoTracking()
            .AnyAsync(ById(valid.Value), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(exists);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TAggregate>>> ListAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var valid = RepositoryGuard.PredicateRequired(predicate);
        if (valid.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TAggregate>>(valid.Errors);
        }

        List<TAggregate> items = await _dbSet
            .AsNoTracking()
            .Where(valid.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result.Success<IReadOnlyList<TAggregate>>(items);
    }

    /// <inheritdoc />
    public async Task<Result<TAggregate>> FirstOrDefaultAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var valid = RepositoryGuard.PredicateRequired(predicate);
        if (valid.IsFailure)
        {
            return Result.Failure<TAggregate>(valid.Errors);
        }

        TAggregate? aggregate = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(valid.Value, cancellationToken)
            .ConfigureAwait(false);

        return aggregate is null
            ? Result.Failure<TAggregate>(
                RepositoryConstant.Constraints.NotFound.Error)
            : Result.Success(aggregate);
    }

    /// <inheritdoc />
    public async Task<Result<int>> CountAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var valid = RepositoryGuard.PredicateRequired(predicate);
        if (valid.IsFailure)
        {
            return Result.Failure<int>(valid.Errors);
        }

        int count = await _dbSet
            .AsNoTracking()
            .CountAsync(valid.Value, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(count);
    }

    #endregion

    #region Helpers

    private static Expression<Func<TAggregate, bool>> ById(TId id)
        => aggregate => EF.Property<TId>(aggregate, nameof(IIdentifiable<TId>.Id)).Equals(id);

    #endregion
}

/// <summary>
/// EF Core repository for aggregates with Guid identifiers.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
public class EfRepository<TAggregate> : EfRepository<TAggregate, Guid>, IRepository<TAggregate>
    where TAggregate : class, IIdentifiable<Guid>
{
    /// <summary>Initializes a new repository over the supplied context.</summary>
    /// <param name="context">The module context that owns the aggregate set.</param>
    public EfRepository(DbContext context)
        : base(context)
    {
    }
}
