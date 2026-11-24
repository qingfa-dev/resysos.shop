namespace BuildingBlock.Specifications.EfCore;

/// <summary>Unified constants and error definitions for the EF Core specification provider.</summary>
public static partial class EfSpecificationConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for a queryable.</summary>
    public static class Query
    {
        /// <summary>Error when no query is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "ef_specification.query.required",
                message: "A source query is required.");
        }
    }

    /// <summary>Constraints and error definitions for a specification.</summary>
    public static class Specification
    {
        /// <summary>Error when no specification is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "ef_specification.specification.required",
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
                code: "ef_specification.evaluator.required",
                message: "An EF Core specification evaluator is required.");
        }
    }

    /// <summary>Constraints and error definitions for provider options.</summary>
    public static class Options
    {
        /// <summary>Error when no options are supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "ef_specification.options.required",
                message: "EF Core specification options are required.");
        }
    }

    /// <summary>Constraints and error definitions for a service collection.</summary>
    public static class Services
    {
        /// <summary>Error when no service collection is supplied.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "ef_specification.services.required",
                message: "A service collection is required.");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the EF Core specification provider.</summary>
    public static class Defaults
    {
        /// <summary>Whether collection includes use split queries by default.</summary>
        public const bool UseSplitQuery = false;

        /// <summary>Whether global query filters are ignored by default.</summary>
        public const bool IgnoreQueryFilters = false;
    }

    #endregion
}
