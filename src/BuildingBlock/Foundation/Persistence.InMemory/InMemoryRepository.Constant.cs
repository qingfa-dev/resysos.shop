namespace BuildingBlock.Persistence.InMemory;

/// <summary>Unified constants and error definitions for the in-memory repository.</summary>
public static partial class InMemoryRepositoryConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions enforced by the in-memory repository.</summary>
    public static class Constraints
    {
        /// <summary>Constraints and error definitions for a missing aggregate.</summary>
        public static class NotFound
        {
            /// <summary>Creates a not-found error describing the aggregate and identifier.</summary>
            /// <typeparam name="TAggregate">The aggregate type.</typeparam>
            /// <typeparam name="TId">The identifier type.</typeparam>
            /// <param name="id">The identifier that was looked up.</param>
            /// <returns>The not-found error.</returns>
            public static Error For<TAggregate, TId>(TId id)
                => Error.NotFound(
                    code: "in_memory_repository.not_found",
                    message: $"{typeof(TAggregate).Name} with id '{id}' was not found.");

            /// <summary>Error when no aggregate matched a predicate.</summary>
            public static class NoMatch
            {
                /// <summary>The error returned when no aggregate matches the predicate.</summary>
                public static readonly Error Error = Error.NotFound(
                    code: "in_memory_repository.no_match",
                    message: "No aggregate matched the predicate.");
            }
        }

        /// <summary>Constraints and error definitions for a duplicate aggregate.</summary>
        public static class Conflict
        {
            /// <summary>Creates a conflict error describing the aggregate and identifier.</summary>
            /// <typeparam name="TAggregate">The aggregate type.</typeparam>
            /// <typeparam name="TId">The identifier type.</typeparam>
            /// <param name="id">The identifier that already exists.</param>
            /// <returns>The conflict error.</returns>
            public static Error AlreadyExists<TAggregate, TId>(TId id)
                => Error.Conflict(
                    code: "in_memory_repository.already_exists",
                    message: $"{typeof(TAggregate).Name} with id '{id}' already exists.");
        }
    }

    #endregion
}
