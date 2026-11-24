namespace BuildingBlock.Specifications.Abstractions;

#region And

/// <summary>
/// Specification that combines two specifications with AND.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    private AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public static Result<Specification<T>> Create(
        ISpecification<T>? left,
        ISpecification<T>? right)
        => SpecificationGuard.NotNull(left, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(validLeft => SpecificationGuard
                .NotNull(right, SpecificationConstant.Constraints.Specification.Required.Error)
                .Map(validRight => (Specification<T>)new AndSpecification<T>(validLeft, validRight)));

    public override Expression<Func<T, bool>> Criteria
    {
        get
        {
            var param = Expression.Parameter(typeof(T), "x");
            var leftBody = Expression.Invoke(_left.Criteria, param);
            var rightBody = Expression.Invoke(_right.Criteria, param);
            var body = Expression.AndAlso(leftBody, rightBody);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}

#endregion
#region Or

/// <summary>
/// Specification that combines two specifications with OR.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    private OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public static Result<Specification<T>> Create(
        ISpecification<T>? left,
        ISpecification<T>? right)
        => SpecificationGuard.NotNull(left, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(validLeft => SpecificationGuard
                .NotNull(right, SpecificationConstant.Constraints.Specification.Required.Error)
                .Map(validRight => (Specification<T>)new OrSpecification<T>(validLeft, validRight)));

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
}

#endregion
#region Not

/// <summary>
/// Specification that negates another specification.
/// Instances are created through <see cref="Create"/> so validation stays on the railway.
/// </summary>
internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification;

    private NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }

    public static Result<Specification<T>> Create(ISpecification<T>? specification)
        => SpecificationGuard
            .NotNull(specification, SpecificationConstant.Constraints.Specification.Required.Error)
            .Map(valid => (Specification<T>)new NotSpecification<T>(valid));

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
