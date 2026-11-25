namespace BuildingBlock.Specifications.Abstractions;

/// <summary>Unified constants and error definitions for the specification concern.</summary>
public static partial class SpecificationConstant
{
    #region Constraints

    public static class Constraints
    {
        /// <summary>Constraints and error definitions for the specification instance.</summary>
        public static class Specification
        {
            /// <summary>Error when a specification instance is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "specification.required",
                    message: "A specification instance is required.");
            }
        }

        /// <summary>Constraints and error definitions for a criteria expression.</summary>
        public static class Criteria
        {
            /// <summary>Error when the criteria expression is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "specification.criteria.required",
                    message: "A criteria expression is required.");
            }
        }

        /// <summary>Constraints and error definitions for an identifier.</summary>
        public static class Id
        {
            /// <summary>Error when the identifier has its default value.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "specification.id.required",
                    message: "A non-default identifier is required.");
            }
        }

        /// <summary>Constraints and error definitions for an identifier collection.</summary>
        public static class Ids
        {
            /// <summary>Error when the identifier collection is missing or empty.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "specification.ids.required",
                    message: "A non-empty identifier collection is required.");
            }
        }

        /// <summary>Constraints and error definitions for paging.</summary>
        public static class Paging
        {
            /// <summary>Error when paging cannot be applied to the specification.</summary>
            public static class NotSupported
            {
                public static readonly Error Error = Error.NotSupported(
                    code: "specification.paging.not_supported",
                    message: "Cannot apply paging to a specification that is not a Specification implementation.");
            }

            /// <summary>Constraints and error definitions for the page number.</summary>
            public static class Page
            {
                /// <summary>Minimum allowed page number.</summary>
                public static class Min
                {
                    public const int Value = 1;

                    public static readonly Error Error = Error.Validation(
                        code: "specification.paging.invalid_page",
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
                        code: "specification.paging.invalid_page_size",
                        message: $"Page size must be at least {Value}.");
                }

                /// <summary>Maximum allowed page size.</summary>
                public static class Max
                {
                    public const int Value = 1000;

                    public static readonly Error Error = Error.Validation(
                        code: "specification.paging.invalid_page_size",
                        message: $"Page size cannot exceed {Value}.");
                }
            }
        }

        /// <summary>Constraints and error definitions for ordering.</summary>
        public static class Ordering
        {
            /// <summary>Error when ordering cannot be applied to the specification.</summary>
            public static class NotSupported
            {
                public static readonly Error Error = Error.NotSupported(
                    code: "specification.ordering.not_supported",
                    message: "Cannot apply ordering to a specification that is not a Specification implementation.");
            }

            /// <summary>Constraints and error definitions for the ordering key selector.</summary>
            public static class KeySelector
            {
                /// <summary>Error when the key selector is missing.</summary>
                public static class Required
                {
                    public static readonly Error Error = Error.Validation(
                        code: "specification.ordering.key_selector.required",
                        message: "An ordering key selector is required.");
                }
            }
        }

        /// <summary>Constraints and error definitions for change tracking.</summary>
        public static class Tracking
        {
            /// <summary>Error when tracking cannot be changed on the specification.</summary>
            public static class NotSupported
            {
                public static readonly Error Error = Error.NotSupported(
                    code: "specification.tracking.not_supported",
                    message: "Cannot disable tracking on a specification that is not a Specification implementation.");
            }
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the specification concern.</summary>
    public static class Defaults
    {
        /// <summary>Fallback page used when no page is specified by a caller.</summary>
        public const int Page = SpecificationConstant.Constraints.Paging.Page.Min.Value;

        /// <summary>Fallback page size used when no page size is specified by a caller.</summary>
        public const int PageSize = 50;

        /// <summary>Fallback ordering direction used by the ordering helpers.</summary>
        public const bool Descending = false;

        /// <summary>Fallback tracking mode for query specifications.</summary>
        public const bool TrackingEnabled = true;
    }

    #endregion
}
