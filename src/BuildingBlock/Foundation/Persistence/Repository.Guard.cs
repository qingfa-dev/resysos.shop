namespace BuildingBlock.Persistence;

/// <summary>
/// Guards for repository operations. Failures travel on the railway instead of throwing.
/// </summary>
public static class RepositoryGuard
{
    #region Lookups

    /// <summary>
    /// Wraps a looked-up aggregate in a result, returning a not-found error when it is missing.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="aggregate">The looked-up aggregate, or null when it is missing.</param>
    /// <param name="id">The identifier that was looked up.</param>
    /// <returns>The aggregate, or a not-found error.</returns>
    public static Result<TAggregate> EnsureFound<TAggregate, TId>(
        TAggregate? aggregate,
        TId id)
        where TAggregate : class
        => aggregate is null
            ? Result.Failure<TAggregate>(
                RepositoryConstant.Constraints.NotFound.For<TAggregate, TId>(id))
            : Result.Success(aggregate);

    #endregion

    #region Validation

    /// <summary>
    /// Checks that an aggregate instance was supplied to a write operation.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="aggregate">The candidate aggregate.</param>
    /// <returns>The aggregate, or a validation error.</returns>
    public static Result<TAggregate> NotNull<TAggregate>(TAggregate? aggregate)
        where TAggregate : class
        => aggregate is null
            ? Result.Failure<TAggregate>(RepositoryConstant.Constraints.Aggregate.Required.Error)
            : Result.Success(aggregate);

    /// <summary>
    /// Checks that a read predicate was supplied.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="predicate">The candidate predicate.</param>
    /// <returns>The predicate, or a validation error.</returns>
    public static Result<Expression<Func<TAggregate, bool>>> PredicateRequired<TAggregate>(
        Expression<Func<TAggregate, bool>>? predicate)
        => predicate is null
            ? Result.Failure<Expression<Func<TAggregate, bool>>>(
                RepositoryConstant.Constraints.Predicate.Required.Error)
            : Result.Success(predicate);

    #endregion

    #region Identifiers

    /// <summary>
    /// Checks that an identifier is not the default value of its type.
    /// </summary>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="id">The candidate identifier.</param>
    /// <returns>The identifier, or a validation error.</returns>
    public static Result<TId> ValidId<TId>(TId id)
        where TId : notnull
        => EqualityComparer<TId>.Default.Equals(id, default!)
            ? Result.Failure<TId>(RepositoryConstant.Constraints.Id.Required.Error)
            : Result.Success(id);

    #endregion

    #region Paging

    /// <summary>
    /// Validates page and page size together and computes the skip/take pair.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>The skip/take pair, or a validation error.</returns>
    public static Result<(int Skip, int Take)> ValidPaging(int page, int pageSize)
    {
        if (page < RepositoryConstant.Constraints.Paging.Page.Min.Value)
        {
            return Result.Failure<(int Skip, int Take)>(
                RepositoryConstant.Constraints.Paging.Page.Min.Error);
        }

        if (pageSize < RepositoryConstant.Constraints.Paging.PageSize.Min.Value)
        {
            return Result.Failure<(int Skip, int Take)>(
                RepositoryConstant.Constraints.Paging.PageSize.Min.Error);
        }

        if (pageSize > RepositoryConstant.Constraints.Paging.PageSize.Max.Value)
        {
            return Result.Failure<(int Skip, int Take)>(
                RepositoryConstant.Constraints.Paging.PageSize.Max.Error);
        }

        (int Skip, int Take) paging = (
            (page - RepositoryConstant.Constraints.Paging.Page.Min.Value) * pageSize,
            pageSize);
        return Result.Success(paging);
    }

    #endregion
}
