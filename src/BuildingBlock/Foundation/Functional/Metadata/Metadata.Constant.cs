namespace BuildingBlock.Functional.Metadata;

public static class MetadataConstant
{
    #region Constants
    public static class WellKnown
    {
        public const string Timestamp = "timestamp";
    }
    #endregion


    #region Constraints
    public static class Constraints
    {
        public static class Key
        {
            public static class Required
            {
                public const bool Value = true;
                public static string Code => $"metadata.key.required";
                public static string Message => $"The metadata key is required.";
            }
            public static class MaxLength
            {
                public const int Value = 255;
                public static string Code => $"metadata.key.max_length";
                public static string Message => $"The maximum length for metadata key is {Value}.";
            }
        }
        public static class Value
        {
            public static class Required
            {
                public const bool Value = true;
                public static string Code => $"metadata.value.required";
                public static string Message => $"The metadata value is required.";
            }
            public static class NotNull
            {
                public const bool Value = true;
                public static string Code => $"metadata.value.not_null";
                public static string Message => $"The metadata value cannot be null.";
            }
        }
        public static class Parameter
        {
            public static class Required
            {
                public const bool Value = true;
                public static string Code(string parameterName) => $"metadata.{parameterName}.required";
                public static string Message(string parameterName) => $"The parameter '{parameterName}' is required.";
            }
            public static class CollectionNotEmpty
            {
                public const bool Value = true;
                public static string Code(string parameterName) => $"metadata.{parameterName}.not_empty";
                public static string Message(string parameterName) => $"The parameter '{parameterName}' must not be empty.";
            }
        }
    }
    #endregion
    #region Defaults
    public static class Defaults
    {
        public const bool ValueRequired = false;
        public const bool ValueNotNull = false;
    }
    #endregion
}