namespace BuildingBlock.Persistence;

/// <summary>Unified constants and error definitions for the repository concern.</summary>
public static partial class RepositoryConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by repositories.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for an aggregate instance.</summary>
        public static class Aggregate
        {
            /// <summary>Error when the aggregate is missing.</summary>
            public static class Required
            {
                /// <summary>The error returned when an aggregate is null.</summary>
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
                /// <summary>The error returned when an identifier is the default value.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "repository.id.required",
                    message: "A non-default identifier is required.");
            }
        }

        /// <summary>Constraints and error definitions for a read predicate.</summary>
        public static class Predicate
        {
            /// <summary>Error when the read predicate is missing.</summary>
            public static class Required
            {
                /// <summary>The error returned when a read predicate is null.</summary>
                public static readonly Error Error = Error.Validation(
                    code: "repository.predicate.required",
                    message: "A read predicate is required.");
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
                    /// <summary>The smallest allowed page number.</summary>
                    public const int Value = 1;

                    /// <summary>The error returned when the page is below the minimum.</summary>
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
                    /// <summary>The smallest allowed page size.</summary>
                    public const int Value = 1;

                    /// <summary>The error returned when the page size is below the minimum.</summary>
                    public static readonly Error Error = Error.Validation(
                        code: "repository.paging.invalid",
                        message: $"Page size must be at least {Value}.");
                }

                /// <summary>Maximum allowed page size.</summary>
                public static class Max
                {
                    /// <summary>The largest allowed page size.</summary>
                    public const int Value = 1000;

                    /// <summary>The error returned when the page size exceeds the maximum.</summary>
                    public static readonly Error Error = Error.Validation(
                        code: "repository.paging.invalid",
                        message: $"Page size cannot exceed {Value}.");
                }
            }
        }

        /// <summary>Constraints and error definitions for a duplicate aggregate.</summary>
        public static class Conflict
        {
            /// <summary>General conflict error.</summary>
            public static readonly Error Error = Error.Conflict(
                code: "repository.conflict",
                message: "The aggregate already exists.");

            /// <summary>Creates a conflict error describing the aggregate and identifier.</summary>
            /// <typeparam name="TAggregate">The aggregate type.</typeparam>
            /// <typeparam name="TId">The identifier type.</typeparam>
            /// <param name="id">The identifier that already exists.</param>
            /// <returns>The conflict error.</returns>
            public static Error AlreadyExists<TAggregate, TId>(TId id)
                => Error.Conflict(
                    code: "repository.conflict",
                    message: $"{typeof(TAggregate).Name} with id '{id}' already exists.");
        }

        /// <summary>Constraints and error definitions for a missing aggregate.</summary>
        public static class NotFound
        {
            /// <summary>General not-found error.</summary>
            public static readonly Error Error = Error.NotFound(
                code: "repository.not_found",
                message: "The requested aggregate was not found.");

            /// <summary>Creates a not-found error describing the aggregate and identifier.</summary>
            /// <typeparam name="TAggregate">The aggregate type.</typeparam>
            /// <typeparam name="TId">The identifier type.</typeparam>
            /// <param name="id">The identifier that was looked up.</param>
            /// <returns>The not-found error.</returns>
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
        public const int Page = Constraints.Paging.Page.Min.Value;

        /// <summary>Fallback page size used when no page size is specified by a caller.</summary>
        public const int PageSize = 50;
    }

    #endregion
}
