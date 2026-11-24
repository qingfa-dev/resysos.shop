namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Result-returning factory and evaluation helpers for specifications.
/// These are the only exposed way to create specifications.
/// </summary>
public static class SpecificationExtensions
{
    #region Evaluation

    /// <summary>
    /// Compiles the specification to a delegate for in-memory evaluation.
    /// </summary>
    public static Result<Func<T, bool>> Compile<T>(this ISpecification<T>? specification)
        => SpecificationGuard.NotNull(
                specification,
                SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(spec => SpecificationGuard.CriteriaRequired(spec.Criteria)
                .Map(criteria => criteria.Compile()));

    #endregion
    #region Ad-hoc Factories

    /// <summary>
    /// Creates a specification from a simple predicate.
    /// </summary>
    public static Result<ISpecification<T>> Create<T>(Expression<Func<T, bool>> criteria)
        => AdHocSpecification<T>.Create(criteria)
            .Map(spec => (ISpecification<T>)spec);

    #endregion
    #region Lookup Factories

    /// <summary>
    /// Creates a specification that matches entities by ID.
    /// </summary>
    public static Result<ISpecification<T>> ById<T, TId>(TId id)
        where T : class, IIdentifiable<TId>
        where TId : notnull
        => SpecificationGuard.IdRequired(id)
            .Bind(valid => Create<T>(x => EqualityComparer<TId>.Default.Equals(x.Id, valid)));

    /// <summary>
    /// Creates a specification that matches entities by IDs.
    /// </summary>
    public static Result<ISpecification<T>> ByIds<T, TId>(IEnumerable<TId> ids)
        where T : class, IIdentifiable<TId>
        where TId : notnull
        => SpecificationGuard.IdsRequired(ids)
            .Bind(set => Create<T>(x => set.Contains(x.Id)));
    #endregion
}
