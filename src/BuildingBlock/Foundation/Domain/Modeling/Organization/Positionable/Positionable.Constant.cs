namespace BuildingBlock.Domain.Modeling.Organization.Positionable;

/// <summary>Unified constants and error definitions for the <see cref="Positionable"/> concern.</summary>
public static partial class PositionableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the positionable entity.</summary>
    public static class Entity
    {
        /// <summary>Error for when the entity is required but null.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "positionable.entity.required",
                message: "Positionable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the position value.</summary>
    public static class Position
    {
        /// <summary>The minimum allowed position value.</summary>
        public static class Min
        {
            public const int Value = 0;

            public static readonly Error Error = Error.Validation(
                code: "positionable.position.negative",
                message: $"Position must not be less than {Value}.");
        }

        /// <summary>The maximum allowed position value.</summary>
        public static class Max
        {
            public const int Value = int.MaxValue;

            public static readonly Error Error = Error.Validation(
                code: "positionable.position.too_large",
                message: $"Position must not exceed {Value}.");
        }

        /// <summary>The step used by increment/decrement helpers.</summary>
        public static class Step
        {
            public const int Value = 1;
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default option values for the <see cref="Positionable"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>The default position for new entities.</summary>
        public const int DefaultPosition = Position.Min.Value;
    }

    #endregion
}
