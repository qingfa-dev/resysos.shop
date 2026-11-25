namespace BuildingBlock.Specifications.Query;

/// <summary>
/// A resolved field path: the member access plus non-null guards for the nullable hops
/// that precede it. Guards keep generated predicates safe for in-memory evaluation.
/// </summary>
/// <param name="Member">The member access expression.</param>
/// <param name="Guards">Non-null checks for the nullable hops on the path.</param>
internal sealed record FieldPath(MemberExpression Member, IReadOnlyList<Expression> Guards);

/// <summary>Resolves contract field names to expression member paths.</summary>
internal static class QueryFieldResolver
{
    #region Resolution

    /// <summary>Resolves a dotted field path to a member expression.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="field">The dotted field path.</param>
    /// <param name="parameter">The lambda parameter used as the path root.</param>
    /// <returns>The resolved path, or a validation error.</returns>
    public static Result<FieldPath> Resolve<T>(string? field, ParameterExpression parameter)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return Result.Failure<FieldPath>(
                QuerySpecificationConstant.Constraints.Field.Required.Error);
        }

        string[] segments = field.Split(QuerySpecificationConstant.Defaults.FieldSeparator);
        if (segments.Any(string.IsNullOrWhiteSpace))
        {
            return Result.Failure<FieldPath>(Unknown<T>(field));
        }

        Expression current = parameter;
        List<Expression> guards = [];

        foreach (string segment in segments)
        {
            PropertyInfo? property = current.Type.GetProperty(
                segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null || property.GetIndexParameters().Length > 0)
            {
                return Result.Failure<FieldPath>(Unknown<T>(field));
            }

            if (IsNullable(current.Type))
            {
                guards.Add(Expression.NotEqual(
                    current,
                    Expression.Constant(null, current.Type)));
            }

            current = Expression.Property(current, property);
        }

        return current is MemberExpression member
            ? Result.Success(new FieldPath(member, guards))
            : Result.Failure<FieldPath>(Unknown<T>(field));
    }

    #endregion

    #region Guards

    /// <summary>Combines a predicate with the non-null guards of its field path.</summary>
    /// <param name="path">The resolved field path.</param>
    /// <param name="body">The predicate built on the leaf member.</param>
    /// <returns>The guarded predicate.</returns>
    public static Expression Guarded(FieldPath path, Expression body)
    {
        Expression result = body;
        for (int index = path.Guards.Count - 1; index >= 0; index--)
        {
            result = Expression.AndAlso(path.Guards[index], result);
        }

        return result;
    }

    /// <summary>
    /// Guards a null check: a null hop makes the leaf member behave as null.
    /// </summary>
    /// <param name="path">The resolved field path.</param>
    /// <param name="body">The null check built on the leaf member.</param>
    /// <returns>The guarded null check.</returns>
    public static Expression NullChecked(FieldPath path, Expression body)
        => path.Guards.Count == 0
            ? body
            : Expression.OrElse(Expression.Not(Combine(path.Guards)), body);

    /// <summary>Builds a null-safe key selector body for ordering.</summary>
    /// <param name="path">The resolved field path.</param>
    /// <returns>The leaf member, or its default value when a hop is null.</returns>
    public static Expression SafeMember(FieldPath path)
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
        => QuerySpecificationConstant.Constraints.Field.Unknown.For(typeof(T).Name, field);

    #endregion
}
