namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Guards for repository operations. Failures travel on the railway instead of throwing.
/// </summary>
public static class RepositoryGuard
{
    #region Lookups

    /// <summary>
    /// Wraps a looked-up aggregate in a result, returning a not-found error when it is missing.
    /// </summary>
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
    public static Result<TAggregate> NotNull<TAggregate>(TAggregate? aggregate)
        where TAggregate : class
        => aggregate is null
            ? Result.Failure<TAggregate>(RepositoryConstant.Constraints.Aggregate.Required.Error)
            : Result.Success(aggregate);

    #endregion
    #region Identifiers

    /// <summary>
    /// Checks that an identifier is not the default value of its type.
    /// </summary>
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
