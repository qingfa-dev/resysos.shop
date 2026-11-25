namespace BuildingBlock.Querying.Security;

/// <summary>Sort-clause validation for <see cref="QuerySecurityGuard"/>.</summary>
public static partial class QuerySecurityGuard
{
    #region Sorts

    /// <summary>Validates each sort clause against the field allowlist.</summary>
    /// <param name="sorts">The sort clauses to validate, or <see langword="null"/> when absent.</param>
    /// <param name="entityType">The entity type the sorts target.</param>
    /// <param name="registry">The field allowlist, or <see langword="null"/> when none is configured.</param>
    /// <param name="errors">The collection that receives every violation.</param>
    public static void Sorts(
        IReadOnlyList<SortDescriptor>? sorts,
        Type entityType,
        QueryFieldRegistry? registry,
        ICollection<Error> errors)
    {
        if (sorts is null)
        {
            return;
        }

        foreach (SortDescriptor sort in sorts)
        {
            if (sort is null)
            {
                continue;
            }

            var resolved = Resolve(registry, entityType, sort.Field);
            if (resolved is null || resolved.IsFailure)
            {
                errors.Add(QuerySecurityConstant.Constraints.Field.NotAllowed(sort.Field));
                continue;
            }

            if (!resolved.Value.Sortable)
            {
                errors.Add(QuerySecurityConstant.Constraints.Sort.NotAllowed(sort.Field));
            }
        }
    }

    #endregion
}
