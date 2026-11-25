namespace BuildingBlock.Querying.Compilation;

/// <summary>Range (<c>between</c>) comparison for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Between

    private static Result<Expression> BuildBetween(QueryFieldPath path, FilterCondition condition)
    {
        if (condition.Values is null || condition.Values.Count < 2)
        {
            return Result.Failure<Expression>(
                QueryValueConverterConstant.Constraints.Value.Required.Error);
        }

        Type propertyType = path.Member.Type;
        Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (!QueryFieldPathResolver.IsOrderable(valueType))
        {
            return Result.Failure<Expression>(
                QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
                    condition.Operator,
                    condition.Field,
                    propertyType));
        }

        var lower = QueryValueConverter.Convert(condition.Values[0], valueType, condition.Field);
        if (lower.IsFailure)
        {
            return Result.Failure<Expression>(lower.Errors);
        }

        var upper = QueryValueConverter.Convert(condition.Values[1], valueType, condition.Field);
        if (upper.IsFailure)
        {
            return Result.Failure<Expression>(upper.Errors);
        }

        Expression comparison = Expression.AndAlso(
            Expression.GreaterThanOrEqual(
                path.Member,
                Constant(propertyType, lower.Value, valueType)),
            Expression.LessThanOrEqual(
                path.Member,
                Constant(propertyType, upper.Value, valueType)));

        return Result.Success(QueryFieldPathResolver.Guarded(path, comparison));
    }

    #endregion
}
