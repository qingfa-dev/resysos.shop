using BuildingBlock.Contracts.Paging;

namespace BuildingBlock.Contracts.Querying;

/// <summary>Unified constants and error definitions for the querying concern.</summary>
public static partial class QueryConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a query request.</summary>
    public static class Constraints
    {
        /// <summary>Error when the query request is missing.</summary>
        public static class Request
        {
            /// <summary>Error when a query request is required but missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "query.request.required",
                    message: "A query request is required.");
            }
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for querying.</summary>
    public static class Defaults
    {
        /// <summary>Default page number.</summary>
        public const int Page = PagingConstant.Defaults.Page;

        /// <summary>Default page size.</summary>
        public const int PageSize = PagingConstant.Defaults.PageSize;
    }

    #endregion
}
