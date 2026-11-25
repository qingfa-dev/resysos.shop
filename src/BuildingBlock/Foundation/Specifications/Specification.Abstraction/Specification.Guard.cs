namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Guards for specification operations. Failures travel on the railway instead of throwing.
/// </summary>
public static class SpecificationGuard
{
    #region Specification

    /// <summary>
    /// Narrows a specification to the concrete <see cref="Specification{T}"/> base,
    /// returning the supplied error on failure.
    /// </summary>
    public static Result<Specification<T>> EnsureConcrete<T>(
        ISpecification<T> specification,
        Error error)
        => specification is Specification<T> concrete
            ? Result.Success(concrete)
            : Result.Failure<Specification<T>>(error);

    #endregion
    #region Factory Inputs

    /// <summary>
    /// Checks that a specification instance was supplied.
    /// </summary>
    public static Result<ISpecification<T>> NotNull<T>(
        ISpecification<T>? specification,
        Error error)
        => specification is null
            ? Result.Failure<ISpecification<T>>(error)
            : Result.Success(specification);

    /// <summary>
    /// Checks that a criteria expression was supplied.
    /// </summary>
    public static Result<Expression<Func<T, bool>>> CriteriaRequired<T>(
        Expression<Func<T, bool>>? criteria)
        => criteria is null
            ? Result.Failure<Expression<Func<T, bool>>>(
                SpecificationConstant.Constraints.Criteria.Required.Error)
            : Result.Success(criteria);

    /// <summary>
    /// Checks that an identifier is not the default value of its type.
    /// </summary>
    public static Result<TId> IdRequired<TId>(TId id)
        => EqualityComparer<TId>.Default.Equals(id, default!)
            ? Result.Failure<TId>(SpecificationConstant.Constraints.Id.Required.Error)
            : Result.Success(id);

    /// <summary>
    /// Checks that a non-empty identifier collection was supplied.
    /// </summary>
    public static Result<HashSet<TId>> IdsRequired<TId>(IEnumerable<TId>? ids)
    {
        if (ids is null)
            return Result.Failure<HashSet<TId>>(SpecificationConstant.Constraints.Ids.Required.Error);

        var set = new HashSet<TId>(ids);
        return set.Count == 0
            ? Result.Failure<HashSet<TId>>(SpecificationConstant.Constraints.Ids.Required.Error)
            : Result.Success(set);
    }

    #endregion
    #region Paging

    /// <summary>
    /// Checks a page number against the documented constraint.
    /// </summary>
    public static Result<int> ValidPage(int page)
        => page >= SpecificationConstant.Constraints.Paging.Page.Min.Value
            ? Result.Success(page)
            : Result.Failure<int>(SpecificationConstant.Constraints.Paging.Page.Min.Error);

    /// <summary>
    /// Checks a page size against the documented constraint.
    /// </summary>
    public static Result<int> ValidPageSize(int pageSize)
        => pageSize < SpecificationConstant.Constraints.Paging.PageSize.Min.Value
            ? Result.Failure<int>(SpecificationConstant.Constraints.Paging.PageSize.Min.Error)
            : pageSize > SpecificationConstant.Constraints.Paging.PageSize.Max.Value
                ? Result.Failure<int>(SpecificationConstant.Constraints.Paging.PageSize.Max.Error)
                : Result.Success(pageSize);

    /// <summary>
    /// Validates page and page size together and computes the skip/take pair.
    /// </summary>
    public static Result<(int Skip, int Take)> ValidPaging(int page, int pageSize)
    {
        var validPage = ValidPage(page);
        if (validPage.IsFailure)
            return Result.Failure<(int Skip, int Take)>(validPage.Errors);

        var validSize = ValidPageSize(pageSize);
        if (validSize.IsFailure)
            return Result.Failure<(int Skip, int Take)>(validSize.Errors);

        (int Skip, int Take) paging = (
            (validPage.Value - 1) * validSize.Value,
            validSize.Value);
        return Result.Success(paging);
    }
    #endregion
}
