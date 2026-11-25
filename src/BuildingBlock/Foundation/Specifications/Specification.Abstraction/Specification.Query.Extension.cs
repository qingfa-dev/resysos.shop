namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Query-shaping extensions. Unsupported implementations and invalid inputs
/// are reported as <see cref="Result{T}"/> failures so callers stay on the railway.
/// </summary>
public static class SpecificationQueryExtensions
{
    #region Paging

    /// <summary>
    /// Adds paging to a specification (skip/take).
    /// </summary>
    public static Result<ISpecification<T>> WithPaging<T>(
        this ISpecification<T> specification,
        int page = SpecificationConstant.Defaults.Page,
        int pageSize = SpecificationConstant.Defaults.PageSize)
        where T : class
        => SpecificationGuard
            .NotNull(specification, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(spec => SpecificationGuard.ValidPaging(page, pageSize)
                .Bind(paging => SpecificationGuard
                    .EnsureConcrete(spec, SpecificationConstant.Constraints.Paging.NotSupported.Error)
                    .Map(concrete =>
                    {
                        concrete.ApplyPaging(paging.Skip, paging.Take);
                        return (ISpecification<T>)concrete;
                    })));

    #endregion
    #region Ordering

    /// <summary>
    /// Adds ordering to a specification.
    /// </summary>
    public static Result<ISpecification<T>> OrderBy<T, TKey>(
        this ISpecification<T> specification,
        Expression<Func<T, TKey>> keySelector,
        bool descending = SpecificationConstant.Defaults.Descending)
        where T : class
        => SpecificationGuard
            .NotNull(specification, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(spec => SpecificationGuard
                .KeySelectorRequired(keySelector)
                .Bind(selector => SpecificationGuard
                    .EnsureConcrete(spec, SpecificationConstant.Constraints.Ordering.NotSupported.Error)
                    .Map(concrete =>
                    {
                        var param = Expression.Parameter(typeof(T), "x");
                        var body = Expression.Invoke(selector, param);
                        var lambda = Expression.Lambda<Func<T, object>>(
                            Expression.Convert(body, typeof(object)),
                            param);

                        if (descending)
                            concrete.AddOrderByDescending(lambda);
                        else
                            concrete.AddOrderBy(lambda);

                        return (ISpecification<T>)concrete;
                    })));

    #endregion
    #region Tracking

    /// <summary>
    /// Disables change tracking for read-only queries.
    /// </summary>
    public static Result<ISpecification<T>> AsNoTracking<T>(this ISpecification<T> specification)
        where T : class
        => SpecificationGuard
            .NotNull(specification, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(spec => SpecificationGuard
                .EnsureConcrete(spec, SpecificationConstant.Constraints.Tracking.NotSupported.Error)
                .Map(concrete =>
                {
                    concrete.DisableTracking();
                    return (ISpecification<T>)concrete;
                }));
    #endregion
}
