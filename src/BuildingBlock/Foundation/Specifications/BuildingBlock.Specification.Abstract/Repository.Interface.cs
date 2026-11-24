using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Base repository interface for aggregate persistence.
/// Every operation stays on the railway: no nullable or throwing variants are exposed.
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
    /// Gets aggregates matching the specification.
    /// </summary>
    Task<Result<IReadOnlyList<TAggregate>>> ListAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first aggregate matching the specification, or a not-found error.
    /// </summary>
    Task<Result<TAggregate>> FirstOrDefaultAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts aggregates matching the specification.
    /// </summary>
    Task<Result<int>> CountAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default);
}

    #endregion
#region Guid Repository

/// <summary>
/// Repository interface for aggregates with Guid identifiers.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
public interface IRepository<TAggregate> : IRepository<TAggregate, Guid>
    where TAggregate : class, IIdentifiable<Guid>
{
#endregion
}
