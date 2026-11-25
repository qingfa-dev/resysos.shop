namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Specification that negates another specification.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class NotSpecification<T> : Specification<T>
{
    #region State

    private readonly ISpecification<T> _specification;

    #endregion

    #region Construction

    private NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }

    #endregion

    #region Factory

    /// <summary>Creates a negated specification, guarding against a null operand.</summary>
    public static Result<Specification<T>> Create(ISpecification<T>? specification)
        => SpecificationGuard
            .NotNull(specification, SpecificationConstant.Constraints.Specification.Required.Error)
            .Map(valid => (Specification<T>)new NotSpecification<T>(valid));

    #endregion

    #region Evaluation

    /// <inheritdoc />
    public override Expression<Func<T, bool>> Criteria
    {
        get
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Invoke(_specification.Criteria, param);
            var notBody = Expression.Not(body);
            return Expression.Lambda<Func<T, bool>>(notBody, param);
        }
    }

    #endregion
}
