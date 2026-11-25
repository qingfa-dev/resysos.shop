namespace BuildingBlock.Querying.Compilation;

/// <summary>Guard and ordering helpers for <see cref="QueryFieldPathResolver"/>.</summary>
internal static partial class QueryFieldPathResolver
{
    #region Guards

    /// <summary>Combines a predicate with the non-null guards of its field path.</summary>
    /// <param name="path">The resolved field path.</param>
    /// <param name="body">The predicate built on the leaf member.</param>
    /// <returns>The guarded predicate.</returns>
    public static Expression Guarded(QueryFieldPath path, Expression body)
    {
        Expression result = body;
        for (int index = path.Guards.Count - 1; index >= 0; index--)
        {
            result = Expression.AndAlso(path.Guards[index], result);
        }

        return result;
    }

    /// <summary>Guards a null check: a null hop makes the leaf member behave as null.</summary>
    /// <param name="path">The resolved field path.</param>
    /// <param name="body">The null check built on the leaf member.</param>
    /// <returns>The guarded null check.</returns>
    public static Expression NullChecked(QueryFieldPath path, Expression body)
        => path.Guards.Count == 0
            ? body
            : Expression.OrElse(Expression.Not(Combine(path.Guards)), body);

    /// <summary>Builds a null-safe key selector body for ordering.</summary>
    /// <param name="path">The resolved field path.</param>
    /// <returns>The leaf member, or its default value when a parent hop is null.</returns>
    public static Expression SafeMember(QueryFieldPath path)
        => path.Guards.Count == 0
            ? path.Member
            : Expression.Condition(
                Combine(path.Guards),
                path.Member,
                Expression.Default(path.Member.Type));

    #endregion

    #region Helpers

    /// <summary>Checks whether a type supports ordering comparison operators.</summary>
    /// <param name="type">The candidate type.</param>
    /// <returns>True when greater/less-than expressions are valid for the type.</returns>
    public static bool IsOrderable(Type type)
        => Type.GetTypeCode(type) is
                TypeCode.Byte or TypeCode.SByte or
                TypeCode.Int16 or TypeCode.UInt16 or
                TypeCode.Int32 or TypeCode.UInt32 or
                TypeCode.Int64 or TypeCode.UInt64 or
                TypeCode.Single or TypeCode.Double or TypeCode.Decimal
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan);

    private static Expression Combine(IReadOnlyList<Expression> guards)
    {
        Expression result = guards[0];
        for (int index = 1; index < guards.Count; index++)
        {
            result = Expression.AndAlso(result, guards[index]);
        }

        return result;
    }

    private static bool IsNullable(Type type)
        => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    private static Error Unknown<T>(string field)
        => QueryFieldPathConstant.Constraints.Field.Unknown(typeof(T).Name, field);

    #endregion
}
