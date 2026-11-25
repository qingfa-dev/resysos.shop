namespace BuildingBlock.Querying.Compilation;

/// <summary>
/// A resolved field path: the member access plus non-null guards for the nullable hops that
/// precede it. Guards keep generated predicates safe for in-memory evaluation when a parent
/// hop is null.
/// </summary>
/// <param name="Member">The member access expression.</param>
/// <param name="Guards">Non-null checks for the nullable hops on the path.</param>
internal sealed record QueryFieldPath(MemberExpression Member, IReadOnlyList<Expression> Guards);

/// <summary>
/// Resolves a registry accessor to a member path and carries the non-null guards for its nullable
/// hops. It is the engine's own copy of the resolution proven in the specification provider: the
/// engine cannot reference that project, so the logic is ported rather than shared.
/// </summary>
/// <remarks>
/// The guard and ordering helpers live in <c>QueryFieldPath.Guard</c>.
/// </remarks>
internal static partial class QueryFieldPathResolver
{
    #region Resolution

    /// <summary>Resolves a registry accessor to a member path rooted at the supplied parameter.</summary>
    /// <typeparam name="T">The entity type the accessor targets.</typeparam>
    /// <param name="accessor">The member accessor that backs the field.</param>
    /// <param name="parameter">The lambda parameter used as the path root.</param>
    /// <param name="field">The external field name, used for diagnostics.</param>
    /// <returns>The resolved path, or a validation error.</returns>
    public static Result<QueryFieldPath> Resolve<T>(
        LambdaExpression? accessor,
        ParameterExpression parameter,
        string field)
    {
        if (accessor is null || parameter is null)
        {
            return Result.Failure<QueryFieldPath>(Unknown<T>(field));
        }

        Expression body = accessor.Body;
        while (body is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } convert)
        {
            body = convert.Operand;
        }

        var members = new List<MemberExpression>();
        Expression current = body;
        while (current is MemberExpression member)
        {
            members.Add(member);
            current = member.Expression!;
        }

        if (current is not ParameterExpression || members.Count == 0)
        {
            return Result.Failure<QueryFieldPath>(Unknown<T>(field));
        }

        members.Reverse();

        Expression built = parameter;
        List<Expression> guards = [];

        foreach (MemberExpression member in members)
        {
            if (member.Member is not PropertyInfo property)
            {
                return Result.Failure<QueryFieldPath>(Unknown<T>(field));
            }

            if (IsNullable(built.Type))
            {
                guards.Add(Expression.NotEqual(
                    built,
                    Expression.Constant(null, built.Type)));
            }

            built = Expression.Property(built, property);
        }

        return built is MemberExpression leaf
            ? Result.Success(new QueryFieldPath(leaf, guards))
            : Result.Failure<QueryFieldPath>(Unknown<T>(field));
    }

    #endregion
}
