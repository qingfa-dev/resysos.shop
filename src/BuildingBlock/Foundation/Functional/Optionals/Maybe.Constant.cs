using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Optionals;

public static class MaybeConstant
{
    #region Constraints

    public static class Constraints
    {
        public static class Value
        {
            public static readonly Error Required = Error.Validation(
                "maybe.value.required",
                "The Maybe value is required but was None.");
        }

        public static class Parameter
        {
            public static readonly Error Required = Error.Validation(
                "maybe.parameter.required",
                "The parameter is required but was null.");
        }
    }

    #endregion
}
