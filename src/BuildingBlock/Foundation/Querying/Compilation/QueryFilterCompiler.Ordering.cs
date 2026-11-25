namespace BuildingBlock.Querying.Compilation;

/// <summary>Ordering comparisons for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Ordering

    private static Result<Expression> BuildOrdering(
        QueryFieldPath path,
        FilterCondition condition,
        Type valueType)
    {
        if (!QueryFieldPathResolver.IsOrderable(valueType))
        {
            return Result.Failure<Expression>(
                QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
                    condition.Operator,
                    condition.Field,
                    path.Member.Type));
        }

        var value = QueryValueConverter.Convert(condition.Value, valueType, condition.Field);
        if (value.IsFailure)
        {
            return Result.Failure<Expression>(value.Errors);
        }

        Expression constant = Constant(path.Member.Type, value.Value, valueType);
        Expression comparison = condition.Operator switch
        {
            FilterOperator.Gt => Expression.GreaterThan(path.Member, constant),
            FilterOperator.Gte => Expression.GreaterThanOrEqual(path.Member, constant),
            FilterOperator.Lt => Expression.LessThan(path.Member, constant),
            FilterOperator.Lte => Expression.LessThanOrEqual(path.Member, constant),
            _ => Expression.Constant(true)
        };

        return Result.Success(QueryFieldPathResolver.Guarded(path, comparison));
    }

    #endregion
}
