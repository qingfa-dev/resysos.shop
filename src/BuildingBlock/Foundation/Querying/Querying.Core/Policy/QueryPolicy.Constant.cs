namespace BuildingBlock.Querying.Policy;

/// <summary>Unified constants and error definitions for query policies.</summary>
public static partial class QueryPolicyConstant
{
    #region Constraints

    /// <summary>Constraints that a usable policy value must satisfy.</summary>
    public static class Constraints
    {
        /// <summary>Constraints for the maximum page size.</summary>
        public static class MaxPageSize
        {
            /// <summary>The smallest usable maximum page size.</summary>
            public const int Min = 1;
        }

        /// <summary>Constraints for the default page size.</summary>
        public static class DefaultPageSize
        {
            /// <summary>The smallest usable default page size.</summary>
            public const int Min = 1;
        }

        /// <summary>Constraints for the maximum filter depth.</summary>
        public static class MaxFilterDepth
        {
            /// <summary>The smallest usable maximum filter depth.</summary>
            public const int Min = 1;
        }

        /// <summary>Constraints for the maximum filter condition count.</summary>
        public static class MaxFilterConditions
        {
            /// <summary>The smallest usable maximum condition count.</summary>
            public const int Min = 0;
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values applied when a policy does not specify its own.</summary>
    public static class Defaults
    {
        /// <summary>The default exposure mode.</summary>
        public const QueryExposure Exposure = QueryExposure.DenyByDefault;

        /// <summary>The default page size applied when a request does not specify one.</summary>
        public const int DefaultPageSize = PagingConstant.Defaults.PageSize;

        /// <summary>The default maximum page size.</summary>
        public const int MaxPageSize = QueryPlanConstant.Defaults.MaxPageSize;

        /// <summary>The default maximum filter nesting depth.</summary>
        public const int MaxFilterDepth = 5;

        /// <summary>The default maximum number of filter conditions.</summary>
        public const int MaxFilterConditions = 50;
    }

    #endregion
}
