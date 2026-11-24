namespace BuildingBlock.Functional.Guards;

/// <summary>Default error definitions used by <see cref="Guard"/>.</summary>
public static partial class GuardConstant
{
    #region Constraints

    /// <summary>Error when a required value is null.</summary>
    public static class Null
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.null",
            message: "Value is required.");
    }

    /// <summary>Error when a required text value is empty or whitespace.</summary>
    public static class Empty
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.empty",
            message: "Value must not be empty or whitespace.");
    }

    /// <summary>Error when a value equals the default for its type.</summary>
    public static class Default
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.default",
            message: "Value must not be the default for its type.");
    }

    /// <summary>Error when a numeric value is negative.</summary>
    public static class Negative
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.negative",
            message: "Value must not be negative.");
    }

    /// <summary>Error when a numeric value is zero or negative.</summary>
    public static class ZeroOrNegative
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.zero_or_negative",
            message: "Value must be greater than zero.");
    }

    /// <summary>Error when a value falls outside an allowed range.</summary>
    public static class OutOfRange
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.out_of_range",
            message: "Value is outside the allowed range.");
    }

    /// <summary>Error when a required collection is empty.</summary>
    public static class EmptyCollection
    {
        public static readonly Error Error = Error.Validation(
            code: "guard.collection.empty",
            message: "Collection must contain at least one item.");
    }

    #endregion
}
