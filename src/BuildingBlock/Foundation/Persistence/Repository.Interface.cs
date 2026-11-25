using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Persistence;

/// <summary>
/// Base repository interface for aggregate persistence.
/// Every operation stays on the railway: no nullable or throwing variants are exposed.
/// Client-driven reads belong to the dynamic query engine; the repository exposes
/// predicate-based reads for domain use.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    #region Lookups

    /// <summary>
    /// Gets an aggregate by its identifier, or a not-found error.
    /// </summary>
    Task<Result<TAggregate>> FindAsync(TId id, CancellationToken cancellationToken = default);

    #endregion

    #region Writes

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    Task<Result<TAggregate>> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    Task<Result<TAggregate>> UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    Task<Result<TAggregate>> RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>
    /// Checks if an aggregate exists by its identifier.
    /// </summary>
    Task<Result<bool>> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregates matching a predicate.
    /// </summary>
    Task<Result<IReadOnlyList<TAggregate>>> ListAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first aggregate matching a predicate, or a not-found error.
    /// </summary>
    Task<Result<TAggregate>> FirstOrDefaultAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts aggregates matching a predicate.
    /// </summary>
    Task<Result<int>> CountAsync(
        Expression<Func<TAggregate, bool>> predicate,
        CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Repository interface for aggregates with Guid identifiers.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
public interface IRepository<TAggregate> : IRepository<TAggregate, Guid>
    where TAggregate : class, IIdentifiable<Guid>
{
}
