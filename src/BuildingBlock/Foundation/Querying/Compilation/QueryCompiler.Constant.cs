namespace BuildingBlock.Querying.Compilation;

/// <summary>Unified constants and error definitions for the query compiler.</summary>
public static partial class QueryCompilerConstant
{
    #region Constraints

    /// <summary>Constraints enforced while compiling a query plan.</summary>
    public static class Constraints
    {
        /// <summary>Constraints for the compiled page size.</summary>
        public static class PageSize
        {
            /// <summary>The minimum page size a compiled plan may carry.</summary>
            public const int Min = 1;

            /// <summary>The page size applied when a policy does not set a usable maximum.</summary>
            public const int Max = QueryPlanConstant.Defaults.MaxPageSize;
        }

        /// <summary>Constraints for the compiled page number.</summary>
        public static class Page
        {
            /// <summary>The minimum page number a compiled plan may carry.</summary>
            public const int Min = QueryPlanConstant.Constraints.Paging.MinPage;
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values applied while compiling a query plan.</summary>
    public static class Defaults
    {
        /// <summary>The page size applied when neither the request nor the policy sets one.</summary>
        public const int DefaultPageSize = PagingConstant.Defaults.PageSize;

        /// <summary>The page size applied when a policy does not set a usable maximum.</summary>
        public const int MaxPageSize = QueryPlanConstant.Defaults.MaxPageSize;

        /// <summary>Whether a compiled offset plan reports a total count.</summary>
        public const bool IncludeTotalCount = QueryPlanConstant.Defaults.IncludeTotalCount;
    }

    #endregion
}
