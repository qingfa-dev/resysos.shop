namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Specification that combines two specifications with OR.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class OrSpecification<T> : Specification<T>
{
    #region State

    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    #endregion

    #region Construction

    private OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    #endregion

    #region Factory

    /// <summary>Creates an OR specification, guarding against null operands.</summary>
    public static Result<Specification<T>> Create(
        ISpecification<T>? left,
        ISpecification<T>? right)
        => SpecificationGuard.NotNull(left, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(validLeft => SpecificationGuard
                .NotNull(right, SpecificationConstant.Constraints.Specification.Required.Error)
                .Map(validRight => (Specification<T>)new OrSpecification<T>(validLeft, validRight)));

    #endregion

    #region Evaluation

    /// <inheritdoc />
    public override Expression<Func<T, bool>> Criteria
    {
        get
        {
            var param = Expression.Parameter(typeof(T), "x");
            var leftBody = Expression.Invoke(_left.Criteria, param);
            var rightBody = Expression.Invoke(_right.Criteria, param);
            var body = Expression.OrElse(leftBody, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    #endregion
}
