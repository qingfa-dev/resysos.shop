namespace BuildingBlock.Querying.Plan;

/// <summary>Unified constants and error definitions for the dynamic query engine.</summary>
public static partial class QueryPlanConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a query plan.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for a plan's paging.</summary>
        public static class Paging
        {
            /// <summary>The minimum page number a plan may carry.</summary>
            public const int MinPage = 1;
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values applied when a query plan does not specify its own.</summary>
    public static class Defaults
    {
        /// <summary>The maximum number of items a single page may request.</summary>
        public const int MaxPageSize = 200;

        /// <summary>The default page number.</summary>
        public const int Page = PagingConstant.Defaults.Page;

        /// <summary>The default page size.</summary>
        public const int PageSize = PagingConstant.Defaults.PageSize;

        /// <summary>The lambda parameter name used when the engine builds field accessors.</summary>
        public const string ParameterName = "x";

        /// <summary>Whether an offset-paged plan reports a total count by default.</summary>
        public const bool IncludeTotalCount = true;
    }

    #endregion
}
