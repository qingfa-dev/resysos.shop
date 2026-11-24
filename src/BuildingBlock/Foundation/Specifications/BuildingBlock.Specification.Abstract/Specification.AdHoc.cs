namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Ad-hoc specification created from a lambda expression.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class AdHocSpecification<T> : Specification<T>
{
    private readonly Expression<Func<T, bool>> _criteria;

    #region Construction

    private AdHocSpecification(Expression<Func<T, bool>> criteria)
    {
        _criteria = criteria;
    }

    #endregion
    #region Factory

    /// <summary>
    /// Creates an ad-hoc specification, returning a validation error for missing criteria.
    /// </summary>
    public static Result<AdHocSpecification<T>> Create(Expression<Func<T, bool>>? criteria)
        => SpecificationGuard.CriteriaRequired(criteria)
            .Map(valid => new AdHocSpecification<T>(valid));

    #endregion
    #region Expression

    public override Expression<Func<T, bool>> Criteria => _criteria;
    #endregion
}
