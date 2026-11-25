namespace BuildingBlock.Specifications.Query;

/// <summary>Filter mapping for <see cref="QuerySpecificationMapper"/>.</summary>
public partial class QuerySpecificationMapper
{
    #region Criteria

    private static Result<Expression<Func<T, bool>>> BuildCriteria<T>(
        IReadOnlyList<FilterDescriptor> filters)
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? body = null;

        foreach (FilterDescriptor filter in filters)
        {
            var predicate = BuildPredicate<T>(parameter, filter);
            if (predicate.IsFailure)
            {
                return Result.Failure<Expression<Func<T, bool>>>(predicate.Errors);
            }

            body = body is null
                ? predicate.Value
                : Expression.AndAlso(body, predicate.Value);
        }

        body ??= Expression.Constant(true);
        return Result.Success(Expression.Lambda<Func<T, bool>>(body, parameter));
    }

    private static Result<Expression> BuildPredicate<T>(
        ParameterExpression parameter,
        FilterDescriptor filter)
        where T : class
        => QueryFieldResolver.Resolve<T>(filter.Field, parameter)
            .Bind(path => BuildComparison(path, filter));

    #endregion

    #region Comparison

    private static Result<Expression> BuildComparison(FieldPath path, FilterDescriptor filter)
    {
        Type propertyType = path.Member.Type;
        Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return filter.Operator switch
        {
            FilterOperator.IsNull => Result.Success<Expression>(
                QueryFieldResolver.NullChecked(path, IsNull(path.Member))),
            FilterOperator.IsNotNull => Result.Success<Expression>(
                QueryFieldResolver.Guarded(path, Expression.Not(IsNull(path.Member)))),
            FilterOperator.In => BuildIn(path, filter),
            FilterOperator.Contains or FilterOperator.StartsWith or FilterOperator.EndsWith
                => BuildString(path, filter),
            FilterOperator.Eq or FilterOperator.Ne
                => BuildEquality(path, filter, valueType),
            FilterOperator.Gt or FilterOperator.Gte or FilterOperator.Lt or FilterOperator.Lte
                => BuildOrdering(path, filter, valueType),
            _ => Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Operator.Unsupported.For(
                    filter.Operator,
                    filter.Field,
                    propertyType))
        };
    }

    private static Result<Expression> BuildEquality(
        FieldPath path,
        FilterDescriptor filter,
        Type valueType)
    {
        var value = QuerySpecificationGuard.ConvertValue(filter.Value, valueType, filter.Field);
        if (value.IsFailure)
        {
            return Result.Failure<Expression>(value.Errors);
        }

        Expression constant = Constant(path.Member.Type, value.Value, valueType);
        Expression comparison = filter.Operator is FilterOperator.Eq
            ? Expression.Equal(path.Member, constant)
            : Expression.NotEqual(path.Member, constant);

        return Result.Success(QueryFieldResolver.Guarded(path, comparison));
    }

    private static Result<Expression> BuildOrdering(
        FieldPath path,
        FilterDescriptor filter,
        Type valueType)
    {
        if (!QueryFieldResolver.IsOrderable(valueType))
        {
            return Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Operator.Unsupported.For(
                    filter.Operator,
                    filter.Field,
                    path.Member.Type));
        }

        var value = QuerySpecificationGuard.ConvertValue(filter.Value, valueType, filter.Field);
        if (value.IsFailure)
        {
            return Result.Failure<Expression>(value.Errors);
        }

        Expression constant = Constant(path.Member.Type, value.Value, valueType);
        Expression comparison = filter.Operator switch
        {
            FilterOperator.Gt => Expression.GreaterThan(path.Member, constant),
            FilterOperator.Gte => Expression.GreaterThanOrEqual(path.Member, constant),
            FilterOperator.Lt => Expression.LessThan(path.Member, constant),
            FilterOperator.Lte => Expression.LessThanOrEqual(path.Member, constant),
            _ => Expression.Constant(true)
        };

        return Result.Success(QueryFieldResolver.Guarded(path, comparison));
    }

    private static Result<Expression> BuildString(FieldPath path, FilterDescriptor filter)
    {
        if (path.Member.Type != typeof(string))
        {
            return Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Operator.Unsupported.For(
                    filter.Operator,
                    filter.Field,
                    path.Member.Type));
        }

        if (filter.Value is null)
        {
            return Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Value.Required.Error);
        }

        MethodInfo? method = filter.Operator switch
        {
            FilterOperator.Contains => typeof(string).GetMethod(
                nameof(string.Contains),
                [typeof(string)]),
            FilterOperator.StartsWith => typeof(string).GetMethod(
                nameof(string.StartsWith),
                [typeof(string)]),
            FilterOperator.EndsWith => typeof(string).GetMethod(
                nameof(string.EndsWith),
                [typeof(string)]),
            _ => null
        };

        if (method is null)
        {
            return Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Operator.Unsupported.For(
                    filter.Operator,
                    filter.Field,
                    path.Member.Type));
        }

        Expression notNull = Expression.NotEqual(
            path.Member,
            Expression.Constant(null, typeof(string)));
        Expression call = Expression.Call(
            path.Member,
            method,
            Expression.Constant(filter.Value));

        return Result.Success(QueryFieldResolver.Guarded(
            path,
            Expression.AndAlso(notNull, call)));
    }

    private static Result<Expression> BuildIn(FieldPath path, FilterDescriptor filter)
    {
        if (filter.Values is null || filter.Values.Count == 0)
        {
            return Result.Failure<Expression>(
                QuerySpecificationConstant.Constraints.Value.Required.Error);
        }

        Type propertyType = path.Member.Type;
        Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        object[] values = new object[filter.Values.Count];

        for (int index = 0; index < filter.Values.Count; index++)
        {
            var converted = QuerySpecificationGuard.ConvertValue(
                filter.Values[index],
                valueType,
                filter.Field);

            if (converted.IsFailure)
            {
                return Result.Failure<Expression>(converted.Errors);
            }

            values[index] = converted.Value;
        }

        Array array = Array.CreateInstance(valueType, values.Length);
        for (int index = 0; index < values.Length; index++)
        {
            array.SetValue(values[index], index);
        }

        Expression target = propertyType == valueType
            ? path.Member
            : Expression.Convert(path.Member, valueType);

        Expression contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [valueType],
            Expression.Constant(array, valueType.MakeArrayType()),
            target);

        if (propertyType != valueType)
        {
            contains = Expression.AndAlso(
                Expression.NotEqual(
                    path.Member,
                    Expression.Constant(null, propertyType)),
                contains);
        }

        return Result.Success(QueryFieldResolver.Guarded(path, contains));
    }

    #endregion

    #region Helpers

    private static Expression Constant(Type propertyType, object value, Type valueType)
        => propertyType == valueType
            ? Expression.Constant(value, propertyType)
            : Expression.Convert(Expression.Constant(value, valueType), propertyType);

    private static Expression IsNull(MemberExpression member)
        => !member.Type.IsValueType || Nullable.GetUnderlyingType(member.Type) is not null
            ? Expression.Equal(member, Expression.Constant(null, member.Type))
            : Expression.Constant(false);

    #endregion
}
