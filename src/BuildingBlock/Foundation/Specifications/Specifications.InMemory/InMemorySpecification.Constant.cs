namespace BuildingBlock.Specifications.InMemory;

/// <summary>Unified constants and error definitions for the in-memory specification provider.</summary>
public static partial class InMemorySpecificationConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a source sequence.</summary>
    public static class Source
    {
        /// <summary>Error when no source sequence is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "in_memory_specification.source.required",
                message: "A source sequence is required.");
        }
    }

    /// <summary>Constraints and error definitions for a specification.</summary>
    public static class Specification
    {
        /// <summary>Error when no specification is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "in_memory_specification.specification.required",
                message: "A specification is required.");
        }
    }

    /// <summary>Constraints and error definitions for an evaluator.</summary>
    public static class Evaluator
    {
        /// <summary>Error when no evaluator is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "in_memory_specification.evaluator.required",
                message: "An in-memory specification evaluator is required.");
        }
    }

    /// <summary>Constraints and error definitions for an aggregate instance.</summary>
    public static class Aggregate
    {
        /// <summary>Error when a null aggregate is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "in_memory_repository.aggregate.required",
                message: "An aggregate instance is required.");
        }
    }

    /// <summary>Constraints and error definitions for an identifier.</summary>
    public static class Id
    {
        /// <summary>Error when a default identifier is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "in_memory_repository.id.required",
                message: "A non-default identifier is required.");
        }
    }

    /// <summary>Constraints and error definitions for a missing aggregate.</summary>
    public static class NotFound
    {
        /// <summary>Creates a not-found error describing the aggregate and identifier.</summary>
        public static Error For<TAggregate, TId>(TId id)
            => Error.NotFound(
                code: "in_memory_repository.not_found",
                message: $"{typeof(TAggregate).Name} with id '{id}' was not found.");

        /// <summary>Error when no aggregate matched a specification.</summary>
        public static class NoMatch
        {
            public static readonly Error Error = Error.NotFound(
                code: "in_memory_repository.no_match",
                message: "No aggregate matched the specification.");
        }
    }

    /// <summary>Constraints and error definitions for a duplicate aggregate.</summary>
    public static class Conflict
    {
        /// <summary>Creates a conflict error describing the aggregate and identifier.</summary>
        public static Error AlreadyExists<TAggregate, TId>(TId id)
            => Error.Conflict(
                code: "in_memory_repository.already_exists",
                message: $"{typeof(TAggregate).Name} with id '{id}' already exists.");
    }

    #endregion
}
