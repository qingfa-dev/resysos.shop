namespace BuildingBlock.Querying.Compilation;

/// <summary>Shared expression helpers for <see cref="QueryFilterCompiler"/>.</summary>
public static partial class QueryFilterCompiler
{
    #region Helpers

    private static Expression Lowered(Expression expression)
        => Expression.Call(
            expression,
            typeof(string).GetMethod(QueryFilterCompilerConstant.Defaults.ToLower, Type.EmptyTypes)!);

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
