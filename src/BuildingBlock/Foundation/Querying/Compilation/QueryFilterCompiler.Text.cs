namespace BuildingBlock.Querying.Compilation;

/// <summary>Text comparisons for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Text

    private static Result<Expression> BuildString(QueryFieldPath path, FilterCondition condition)
    {
        if (path.Member.Type != typeof(string))
        {
            return Result.Failure<Expression>(
                QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
                    condition.Operator,
                    condition.Field,
                    path.Member.Type));
        }

        if (condition.Value is null)
        {
            return Result.Failure<Expression>(
                QueryValueConverterConstant.Constraints.Value.Required.Error);
        }

        MethodInfo? method = condition.Operator switch
        {
            FilterOperator.Contains => typeof(string).GetMethod(
                QueryFilterCompilerConstant.Defaults.Contains,
                [typeof(string)]),
            FilterOperator.StartsWith => typeof(string).GetMethod(
                QueryFilterCompilerConstant.Defaults.StartsWith,
                [typeof(string)]),
            FilterOperator.EndsWith => typeof(string).GetMethod(
                QueryFilterCompilerConstant.Defaults.EndsWith,
                [typeof(string)]),
            _ => null
        };

        if (method is null)
        {
            return Result.Failure<Expression>(
                QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
                    condition.Operator,
                    condition.Field,
                    path.Member.Type));
        }

        Expression notNull = Expression.NotEqual(
            path.Member,
            Expression.Constant(null, typeof(string)));
        Expression target = condition.CaseInsensitive
            ? Lowered(path.Member)
            : path.Member;
        Expression argument = condition.CaseInsensitive
            ? Lowered(Expression.Constant(condition.Value))
            : Expression.Constant(condition.Value);
        Expression call = Expression.Call(target, method, argument);

        return Result.Success(QueryFieldPathResolver.Guarded(
            path,
            Expression.AndAlso(notNull, call)));
    }

    #endregion
}
