namespace BuildingBlock.Querying.Compilation;

/// <summary>Equality comparisons for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Equality

    private static Result<Expression> BuildEquality(
        QueryFieldPath path,
        FilterCondition condition,
        Type valueType)
    {
        var value = QueryValueConverter.Convert(condition.Value, valueType, condition.Field);
        if (value.IsFailure)
        {
            return Result.Failure<Expression>(value.Errors);
        }

        Expression comparison;
        if (condition.CaseInsensitive && valueType == typeof(string))
        {
            Expression notNull = Expression.NotEqual(
                path.Member,
                Expression.Constant(null, typeof(string)));
            Expression equal = Expression.Equal(
                Lowered(path.Member),
                Lowered(Expression.Constant(value.Value, typeof(string))));

            comparison = condition.Operator is FilterOperator.Eq
                ? Expression.AndAlso(notNull, equal)
                : Expression.Not(Expression.AndAlso(notNull, equal));
        }
        else
        {
            Expression constant = Constant(path.Member.Type, value.Value, valueType);
            comparison = condition.Operator is FilterOperator.Eq
                ? Expression.Equal(path.Member, constant)
                : Expression.NotEqual(path.Member, constant);
        }

        return Result.Success(QueryFieldPathResolver.Guarded(path, comparison));
    }

    #endregion
}
