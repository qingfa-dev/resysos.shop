namespace BuildingBlock.Domain.Modeling.Lifecycle;

/// <summary>Shared constants for the <see cref="Lifecycle"/> concerns.</summary>
public static class LifecycleConstant
{
    #region Constraints

    /// <summary>Actor name constraints shared by all lifecycle concerns.</summary>
    public static class Actor
    {
        /// <summary>Minimum length for an actor name.</summary>
        public static class MinLength
        {
            public const int Value = 2;
        }

        /// <summary>Maximum length for an actor name.</summary>
        public static class MaxLength
        {
            public const int Value = 256;
        }

        /// <summary>Format constraint for an actor name.</summary>
        public static class Format
        {
            public const string Pattern = @"^\S(?:.*\S)?$";
        }
    }

    /// <summary>Timestamp constraints shared by all lifecycle concerns.</summary>
    public static class Timestamp
    {
        /// <summary>Maximum allowed clock skew for timestamps in the future, in seconds.</summary>
        public static class MaxFutureSkew
        {
            public const int Value = 300;
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default option values for the <see cref="Lifecycle"/> concern.</summary>
    public static class Defaults
    {
        /// <summary>Default value for requiring an actor on lifecycle operations.</summary>
        public const bool RequireActor = false;
    }

    #endregion
}
