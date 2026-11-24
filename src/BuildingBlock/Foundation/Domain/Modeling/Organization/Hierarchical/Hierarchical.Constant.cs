namespace BuildingBlock.Domain.Modeling.Organization.Hierarchical;

/// <summary>Unified constants and error definitions for the <see cref="Hierarchical"/> concern.</summary>
/// <remarks>
/// Cycle detection note: the building block only sees a single <c>ParentId</c>
/// assignment. Callers that own the tree must pass the entity id and its
/// ancestor chain to the <c>ValidateParent</c> overloads so self-parent and
/// cycle assignments can be rejected before mutation.
/// </remarks>
public static partial class HierarchicalConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the hierarchical entity.</summary>
    public static class Entity
    {
        /// <summary>Error for when the entity is required but null.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "hierarchical.entity.required",
                message: "Hierarchical entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the parent assignment.</summary>
    public static class Parent
    {
        /// <summary>Error for when the parent identifier is empty.</summary>
        public static class Empty
        {
            public static readonly Error Error = Error.Validation(
                code: "hierarchical.parent.empty",
                message: "Parent identifier must not be empty. Use null for root entities.");
        }

        /// <summary>Error for when the entity is assigned as its own parent.</summary>
        public static class Self
        {
            public static readonly Error Error = Error.Validation(
                code: "hierarchical.parent.self_parent",
                message: "Entity cannot be its own parent.");
        }

        /// <summary>Error for when the parent assignment would create a cycle.</summary>
        public static class Cycle
        {
            public static readonly Error Error = Error.Validation(
                code: "hierarchical.parent.cycle_detected",
                message: "Parent assignment would create a cycle.");
        }

        /// <summary>Maximum allowed materialized path length in characters.</summary>
        public static class MaxPathLength
        {
            public const int Value = 1024;

            public static readonly Error Error = Error.Validation(
                code: "hierarchical.path.too_long",
                message: $"Path cannot exceed {Value} characters.");
        }
    }

    /// <summary>Constraints and error definitions for the hierarchy level.</summary>
    public static class Level
    {
        /// <summary>Minimum allowed level (root).</summary>
        public static class Min
        {
            public const int Value = 0;
        }

        /// <summary>Maximum allowed level and depth.</summary>
        public static class Max
        {
            public const int Value = 32;

            public static readonly Error DepthExceeded = Error.Validation(
                code: "hierarchical.depth.exceeded",
                message: $"Hierarchy depth cannot exceed {Value} levels.");
        }

        /// <summary>Error for when the level is outside the allowed range.</summary>
        public static class Range
        {
            public static readonly Error Error = Error.Validation(
                code: "hierarchical.level.out_of_range",
                message: $"Level must be between {Min.Value} and {Max.Value}.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default option values for the <see cref="Hierarchical"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Default path separator for materialized paths.</summary>
        public const char PathSeparator = '/';

        /// <summary>Default level for root entities.</summary>
        public const int RootLevel = Level.Min.Value;
    }

    #endregion

    #region Patterns

    /// <summary>Regex patterns for the <see cref="Hierarchical"/> concern.</summary>
    public static class Patterns
    {
        /// <summary>Pattern for materialized path validation (segments separated by '/').</summary>
        public const string Path = @"^[^/]+(?:/[^/]+)*$";
    }

    #endregion
}
