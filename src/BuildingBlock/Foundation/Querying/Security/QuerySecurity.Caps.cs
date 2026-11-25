namespace BuildingBlock.Querying.Security;

/// <summary>Shape-cap validation for <see cref="QuerySecurityGuard"/>.</summary>
public static partial class QuerySecurityGuard
{
    #region Caps

    /// <summary>Checks the measured filter shape against the policy caps.</summary>
    /// <param name="depth">The depth of the filter tree.</param>
    /// <param name="conditions">The number of leaf conditions in the filter tree.</param>
    /// <param name="policy">The policy supplying the caps.</param>
    /// <param name="errors">The collection that receives every violation.</param>
    public static void Caps(int depth, int conditions, QueryPolicy policy, ICollection<Error> errors)
    {
        if (depth > policy.MaxFilterDepth)
        {
            errors.Add(QuerySecurityConstant.Constraints.Filter.TooDeep(depth, policy.MaxFilterDepth));
        }

        if (conditions > policy.MaxFilterConditions)
        {
            errors.Add(QuerySecurityConstant.Constraints.Filter.TooManyConditions(conditions, policy.MaxFilterConditions));
        }
    }

    #endregion
}
