namespace BuildingBlock.Specifications.Abstractions;

/// <summary>Unified constants and error definitions for the repository concern.</summary>
public static partial class RepositoryConstant
{
    #region Constraints

    public static class Constraints
    {
        /// <summary>Constraints and error definitions for an aggregate instance.</summary>
        public static class Aggregate
        {
            /// <summary>Error when the aggregate is missing.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "repository.aggregate.required",
                    message: "An aggregate instance is required.");
            }
        }

        /// <summary>Constraints and error definitions for an identifier.</summary>
        public static class Id
        {
            /// <summary>Error when the identifier has its default value.</summary>
            public static class Required
            {
                public static readonly Error Error = Error.Validation(
                    code: "repository.id.required",
                    message: "A non-default identifier is required.");
            }
        }

        /// <summary>Constraints and error definitions for paging.</summary>
        public static class Paging
        {
            /// <summary>Constraints and error definitions for the page number.</summary>
            public static class Page
            {
                /// <summary>Minimum allowed page number.</summary>
                public static class Min
                {
                    public const int Value = 1;

                    public static readonly Error Error = Error.Validation(
                        code: "repository.paging.invalid",
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
                        code: "repository.paging.invalid",
                        message: $"Page size must be at least {Value}.");
                }

                /// <summary>Maximum allowed page size.</summary>
                public static class Max
                {
                    public const int Value = 1000;

                    public static readonly Error Error = Error.Validation(
                        code: "repository.paging.invalid",
                        message: $"Page size cannot exceed {Value}.");
                }
            }
        }

        /// <summary>Constraints and error definitions for a missing aggregate.</summary>
        public static class NotFound
        {
            /// <summary>General not-found error.</summary>
            public static readonly Error Error = Error.NotFound(
                code: "repository.not_found",
                message: "The requested aggregate was not found.");

            /// <summary>Creates a not-found error describing the aggregate and identifier.</summary>
            public static Error For<TAggregate, TId>(TId id)
                => Error.NotFound(
                    code: "repository.not_found",
                    message: $"{typeof(TAggregate).Name} with id '{id}' was not found.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the repository concern.</summary>
    public static class Defaults
    {
        /// <summary>Fallback page used when no page is specified by a caller.</summary>
        public const int Page = RepositoryConstant.Constraints.Paging.Page.Min.Value;

        /// <summary>Fallback page size used when no page size is specified by a caller.</summary>
        public const int PageSize = 50;
    }

    #endregion
}
