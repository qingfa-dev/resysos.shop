namespace BuildingBlock.Querying.Security;

/// <summary>
/// Walks a filter tree and its sort clauses against a policy's field allowlist, recording every
/// security violation as a named <c>query.security.*</c> error. The walk never throws: an unknown
/// field, a disallowed operator, or a cap breach all stay on the railway.
/// </summary>
/// <remarks>
/// Concerns live in sibling partials: <c>QuerySecurity.Filter</c> (filter tree),
/// <c>QuerySecurity.Sort</c> (sort clauses) and <c>QuerySecurity.Caps</c> (shape caps).
/// </remarks>
public static partial class QuerySecurityGuard
{
    #region Helpers

    private static Result<QueryField>? Resolve(
        QueryFieldRegistry? registry,
        Type entityType,
        string? field)
        => registry?.Resolve(entityType, field ?? string.Empty);

    private static FilterOperators ToMask(FilterOperator @operator)
        => Enum.IsDefined(@operator)
            ? (FilterOperators)(1 << (int)@operator)
            : FilterOperators.None;

    #endregion
}
