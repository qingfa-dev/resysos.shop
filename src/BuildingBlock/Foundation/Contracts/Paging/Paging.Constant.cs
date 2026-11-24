namespace BuildingBlock.Contracts.Paging;

/// <summary>Unified constants and error definitions for paging.</summary>
public static partial class PagingConstant
{
    #region Constraints

    public static class Constraints
    {

        /// <summary>Constraints and error definitions for the page number.</summary>
        public static class Page
        {
            /// <summary>Minimum allowed page number.</summary>
            public static class Min
            {
                public const int Value = 1;

                public static readonly Error Error = Error.Validation(
                    code: "paging.page.invalid",
                    message: $"Page must be at least {Value}.");
            }
        }

        /// <summary>Constraints and error definitions for the page size.</summary>
        public static class PageSize
        {
            /// <summary>Minimum allowed page size.</summary>
            public static class Min
            {
                public const int Value = 1;

                public static readonly Error Error = Error.Validation(
                    code: "paging.page_size.invalid",
                    message: $"Page size must be at least {Value}.");
            }

            /// <summary>Maximum allowed page size.</summary>
            public static class Max
            {
                public const int Value = 200;

                public static readonly Error Error = Error.Validation(
                    code: "paging.page_size.invalid",
                    message: $"Page size cannot exceed {Value}.");
            }
        }

    }

    #endregion

    #region Defaults

    /// <summary>Default values for paging.</summary>
    public static class Defaults
    {
        /// <summary>Default page number.</summary>
        public const int Page = PagingConstant.Constraints.Page.Min.Value;

        /// <summary>Default page size.</summary>
        public const int PageSize = 20;
    }

    #endregion
}
