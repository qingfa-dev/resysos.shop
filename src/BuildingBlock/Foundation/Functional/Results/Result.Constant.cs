using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Results;

public static class ResultConstant
{
    #region Constants

    public static class Constraints
    {
        public static class Value
        {
            public static readonly Error Required = Error.Validation(
                code: "result.value.required",
                message: "Value is required");

            public static readonly Error Invalid = Error.Validation(
                code: "result.value.invalid",
                message: "A failed result does not contain a value. Inspect Errors or use Match.");
        }

        public static class Errors
        {
            public static readonly Error Required = Error.Validation(
                code: "result.errors.required",
                message: "Errors are required");

            public static readonly Error Empty = Error.Validation(
                code: "result.errors.empty",
                message: "A failure result must contain at least one error.");

            public static readonly Error ContainsNull = Error.Validation(
                code: "result.errors.contains_null",
                message: "A failure result cannot contain a null error.");
        }

        public static class Filter
        {
            public static readonly Error Unsatisfied = Error.Validation(
                code: "result.filter.unsatisfied",
                message: "The value did not satisfy the filter predicate.");
        }

        public static class Parameter
        {
            public static readonly Error Required = Error.Validation(
                code: "result.parameter.required",
                message: "A required parameter was null.");
        }
    }
    #endregion
}
