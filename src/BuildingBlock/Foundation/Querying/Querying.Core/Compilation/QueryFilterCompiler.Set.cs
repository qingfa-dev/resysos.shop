namespace BuildingBlock.Querying.Compilation;

/// <summary>Set membership (<c>in</c>/<c>notin</c>) for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Set

    private static Result<Expression> BuildSet(
        QueryFieldPath path,
        FilterCondition condition,
        bool negate)
    {
        if (condition.Values is null || condition.Values.Count == 0)
        {
            return Result.Failure<Expression>(
                QueryValueConverterConstant.Constraints.Value.Required.Error);
        }

        Type propertyType = path.Member.Type;
        Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        bool insensitive = condition.CaseInsensitive && valueType == typeof(string);
        object[] values = new object[condition.Values.Count];

        for (int index = 0; index < condition.Values.Count; index++)
        {
            var converted = QueryValueConverter.Convert(
                condition.Values[index],
                valueType,
                condition.Field);

            if (converted.IsFailure)
            {
                return Result.Failure<Expression>(converted.Errors);
            }

            values[index] = insensitive
                ? ((string)converted.Value).ToLower()
                : converted.Value;
        }

        Array array = Array.CreateInstance(valueType, values.Length);
        for (int index = 0; index < values.Length; index++)
        {
            array.SetValue(values[index], index);
        }

        Expression target = propertyType == valueType
            ? path.Member
            : Expression.Convert(path.Member, valueType);

        if (insensitive)
        {
            target = Lowered(target);
        }

        Expression contains = Expression.Call(
            typeof(Enumerable),
            QueryFilterCompilerConstant.Defaults.EnumerableContains,
            [valueType],
            Expression.Constant(array, valueType.MakeArrayType()),
            target);

        Expression membership = negate ? Expression.Not(contains) : contains;

        bool nullableLeaf = !propertyType.IsValueType
            || Nullable.GetUnderlyingType(propertyType) is not null;

        Expression body = nullableLeaf
            ? Expression.AndAlso(
                Expression.NotEqual(
                    path.Member,
                    Expression.Constant(null, propertyType)),
                membership)
            : membership;

        return Result.Success(QueryFieldPathResolver.Guarded(path, body));
    }

    #endregion
}
