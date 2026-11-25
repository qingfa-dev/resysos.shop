namespace BuildingBlock.Querying.Security;

/// <summary>
/// The security gate for a compiled query. It validates a filter tree and its sort clauses against
/// the policy's field allowlist, per-field operator allowlist, and caps before execution. Every
/// rejection is a named <c>query.security.*</c> error and the gate never throws.
/// </summary>
/// <remarks>
/// Depth is the number of nodes on the longest root-to-leaf path: a condition is depth 1 and a
/// group or negation adds one. Conditions are the leaf conditions of the whole tree.
/// </remarks>
public static partial class QuerySecurity
{
    #region Validate

    /// <summary>Validates a filter tree and sort clauses against a query policy.</summary>
    /// <param name="filter">The filter tree to validate, or <see langword="null"/> when absent.</param>
    /// <param name="sorts">The sort clauses to validate.</param>
    /// <param name="policy">The policy supplying the allowlist and caps.</param>
    /// <param name="entityType">The entity type the filter and sorts target.</param>
    /// <param name="context">The caller context, reserved for role-scoped grants.</param>
    /// <returns>
    /// Success when every clause is allowed; otherwise the collected <c>query.security.*</c>
    /// violations. A missing policy fails closed: it is treated as an empty allowlist.
    /// </returns>
    public static Result<Unit> Validate(
        FilterNode? filter,
        IReadOnlyList<SortDescriptor> sorts,
        QueryPolicy policy,
        Type entityType,
        QueryContext context)
    {
        if (policy is null)
        {
            policy = new QueryPolicy();
        }

        List<Error> errors = [];

        var (depth, conditions) = QuerySecurityGuard.Filter(filter, entityType, policy.Registry, errors);
        QuerySecurityGuard.Sorts(sorts, entityType, policy.Registry, errors);
        QuerySecurityGuard.Caps(depth, conditions, policy, errors);

        return errors.Count == 0
            ? Result.Success(Unit.Value)
            : Result.Failure<Unit>(errors);
    }

    #endregion
}
