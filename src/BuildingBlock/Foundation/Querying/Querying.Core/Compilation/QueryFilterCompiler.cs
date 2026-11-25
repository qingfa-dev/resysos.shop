namespace BuildingBlock.Querying.Compilation;

/// <summary>
/// Compiles a filter tree into a <see cref="Expression{TDelegate}"/> predicate. Each condition
/// resolves its registered accessor, converts the raw value to the field type, builds the
/// comparison for the operator, and ANDs the non-null guards of the path so nested hops are safe
/// for in-memory evaluation. Groups combine with <c>AndAlso</c>/<c>OrElse</c> and negation wraps
/// in <c>Not</c>. Failures stay on the railway; compilation never throws.
/// </summary>
/// <remarks>
/// Operator families live in sibling partials: <c>QueryFilterCompiler.Equality</c>,
/// <c>.Ordering</c>, <c>.Text</c>, <c>.Set</c>, <c>.Between</c> and <c>.Helpers</c>.
/// </remarks>
public static partial class QueryFilterCompiler
{
    #region Compile

    /// <summary>Compiles a filter tree into a predicate for an entity type.</summary>
    /// <typeparam name="T">The entity type the filter targets.</typeparam>
    /// <param name="filter">The filter tree, or <see langword="null"/> to match everything.</param>
    /// <param name="registry">The field allowlist the filter is resolved against.</param>
    /// <param name="context">The caller context, reserved for role-scoped grants.</param>
    /// <returns>The predicate, or the first validation error encountered.</returns>
    /// <remarks>
    /// This compiler does not enforce the policy's security gate. Callers must run
    /// <see cref="QuerySecurity.Validate"/> first; the <see cref="QueryCompiler"/> facade does.
    /// </remarks>
    public static Result<Expression<Func<T, bool>>> Compile<T>(
        FilterNode? filter,
        QueryFieldRegistry registry,
        QueryContext context)
    {
        var parameter = Expression.Parameter(typeof(T), QueryPlanConstant.Defaults.ParameterName);

        return CompileNode<T>(filter, registry, context, parameter)
            .Map(body => Expression.Lambda<Func<T, bool>>(body, parameter));
    }

    #endregion

    #region Nodes

    private static Result<Expression> CompileNode<T>(
        FilterNode? node,
        QueryFieldRegistry? registry,
        QueryContext context,
        ParameterExpression parameter)
        => node switch
        {
            FilterCondition condition => CompileCondition<T>(condition, registry, parameter),
            FilterGroup group => CompileGroup<T>(group, registry, context, parameter),
            FilterNegation negation => CompileNode<T>(negation.Child, registry, context, parameter)
                .Map(static body => (Expression)Expression.Not(body)),
            _ => Result.Success<Expression>(Expression.Constant(true))
        };

    private static Result<Expression> CompileGroup<T>(
        FilterGroup group,
        QueryFieldRegistry? registry,
        QueryContext context,
        ParameterExpression parameter)
    {
        bool isOr = group.Operator is FilterGroupOperator.Or;
        Expression? combined = null;

        // A hand-built group can carry a null collection; treat it as empty so compilation never
        // dereferences null on client-supplied input.
        if (group.Children is not null)
        {
            foreach (FilterNode child in group.Children)
            {
                var compiled = CompileNode<T>(child, registry, context, parameter);
                if (compiled.IsFailure)
                {
                    return Result.Failure<Expression>(compiled.Errors);
                }

                combined = combined is null
                    ? compiled.Value
                    : isOr
                        ? Expression.OrElse(combined, compiled.Value)
                        : Expression.AndAlso(combined, compiled.Value);
            }
        }

        return Result.Success<Expression>(
            combined ?? Expression.Constant(!isOr));
    }

    #endregion

    #region Conditions

    private static Result<Expression> CompileCondition<T>(
        FilterCondition condition,
        QueryFieldRegistry? registry,
        ParameterExpression parameter)
    {
        if (registry is null)
        {
            return Result.Failure<Expression>(
                QueryFieldRegistryConstant.Constraints.Field.NotAllowed(condition.Field));
        }

        return registry.Resolve(typeof(T), condition.Field)
            .Bind(field => QueryFieldPathResolver.Resolve<T>(field.Accessor, parameter, condition.Field))
            .Bind(path => BuildComparison(path, condition));
    }

    #endregion

    #region Comparison

    private static Result<Expression> BuildComparison(QueryFieldPath path, FilterCondition condition)
    {
        Type propertyType = path.Member.Type;
        Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return condition.Operator switch
        {
            FilterOperator.IsNull => Result.Success<Expression>(
                QueryFieldPathResolver.NullChecked(path, IsNull(path.Member))),
            FilterOperator.IsNotNull => Result.Success<Expression>(
                QueryFieldPathResolver.Guarded(path, Expression.Not(IsNull(path.Member)))),
            FilterOperator.In => BuildSet(path, condition, negate: false),
            FilterOperator.NotIn => BuildSet(path, condition, negate: true),
            FilterOperator.Between => BuildBetween(path, condition),
            FilterOperator.Contains or FilterOperator.StartsWith or FilterOperator.EndsWith
                => BuildString(path, condition),
            FilterOperator.Eq or FilterOperator.Ne
                => BuildEquality(path, condition, valueType),
            FilterOperator.Gt or FilterOperator.Gte or FilterOperator.Lt or FilterOperator.Lte
                => BuildOrdering(path, condition, valueType),
            _ => Result.Failure<Expression>(
                QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
                    condition.Operator,
                    condition.Field,
                    propertyType))
        };
    }

    #endregion
}
