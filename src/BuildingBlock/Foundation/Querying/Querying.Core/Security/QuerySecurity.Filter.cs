namespace BuildingBlock.Querying.Security;

/// <summary>Filter-tree validation for <see cref="QuerySecurityGuard"/>.</summary>
public static partial class QuerySecurityGuard
{
    #region Filter

    /// <summary>Validates a filter tree and reports its depth and condition count.</summary>
    /// <param name="filter">The filter tree to walk, or <see langword="null"/> when absent.</param>
    /// <param name="entityType">The entity type the filter targets.</param>
    /// <param name="registry">The field allowlist, or <see langword="null"/> when none is configured.</param>
    /// <param name="errors">The collection that receives every violation.</param>
    /// <returns>The depth of the tree and the number of leaf conditions it contains.</returns>
    public static (int Depth, int Conditions) Filter(
        FilterNode? filter,
        Type entityType,
        QueryFieldRegistry? registry,
        ICollection<Error> errors)
        => filter is null
            ? (0, 0)
            : Walk(filter, entityType, registry, errors);

    #endregion

    #region Walk

    private static (int Depth, int Conditions) Walk(
        FilterNode node,
        Type entityType,
        QueryFieldRegistry? registry,
        ICollection<Error> errors)
    {
        switch (node)
        {
            case FilterCondition condition:
                Condition(condition, entityType, registry, errors);
                return (1, 1);

            case FilterGroup group:
                return Group(group, entityType, registry, errors);

            case FilterNegation negation:
            {
                var (depth, conditions) = Walk(negation.Child, entityType, registry, errors);
                return (depth + 1, conditions);
            }

            default:
                return (1, 0);
        }
    }

    private static (int Depth, int Conditions) Group(
        FilterGroup group,
        Type entityType,
        QueryFieldRegistry? registry,
        ICollection<Error> errors)
    {
        int depth = 1;
        int conditions = 0;

        // A hand-built or JSON-bound group can carry a null collection; treat it as empty so the
        // walk never dereferences null on client-supplied input.
        if (group.Children is null)
        {
            return (depth, conditions);
        }

        foreach (FilterNode child in group.Children)
        {
            var (childDepth, childConditions) = Walk(child, entityType, registry, errors);
            depth = Math.Max(depth, childDepth + 1);
            conditions += childConditions;
        }

        return (depth, conditions);
    }

    #endregion

    #region Conditions

    private static void Condition(
        FilterCondition condition,
        Type entityType,
        QueryFieldRegistry? registry,
        ICollection<Error> errors)
    {
        var resolved = Resolve(registry, entityType, condition.Field);
        if (resolved is null || resolved.IsFailure)
        {
            errors.Add(QuerySecurityConstant.Constraints.Field.NotAllowed(condition.Field));
            return;
        }

        if ((resolved.Value.Filterable & ToMask(condition.Operator)) == 0)
        {
            errors.Add(QuerySecurityConstant.Constraints.Operator.NotAllowed(condition.Field, condition.Operator));
        }
    }

    #endregion
}
