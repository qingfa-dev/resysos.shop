namespace BuildingBlock.Functional.Metadata;

public static class MetadataGuard
{
    #region Keys
    public static void KeyRequired(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                ErrorMessage(
                    MetadataConstant.Constraints.Key.Required.Code,
                    MetadataConstant.Constraints.Key.Required.Message), nameof(key));
        }
    }
    public static void KeyMaxLength(string key)
    {
        if (key.Length > MetadataConstant.Constraints.Key.MaxLength.Value)
        {
            throw new ArgumentException(
                ErrorMessage(
                    MetadataConstant.Constraints.Key.MaxLength.Code,
                    MetadataConstant.Constraints.Key.MaxLength.Message), nameof(key));
        }
    }
    public static void KeyValid(string key)
    {
        KeyRequired(key);
        KeyMaxLength(key);
    }
    #endregion

    #region Values
    public static void ValueRequired(string? value, bool isRequired = MetadataConstant.Defaults.ValueRequired)
    {
        if (isRequired && string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                ErrorMessage(
                    MetadataConstant.Constraints.Value.Required.Code,
                    MetadataConstant.Constraints.Value.Required.Message), nameof(value));
        }
    }

    public static void ValueNotNull(object? value, bool isRequired = MetadataConstant.Defaults.ValueNotNull)
    {
        if (isRequired && value is null)
        {
            throw new ArgumentNullException(nameof(value), ErrorMessage(
                    MetadataConstant.Constraints.Value.NotNull.Code,
                    MetadataConstant.Constraints.Value.NotNull.Message));
        }
    }
    #endregion

    #region Collections
    public static void CollectionRequired<T>(IEnumerable<T>? collection, string parameterName, bool isRequired = true)
    {
        if (isRequired && collection is null)
        {
            throw new ArgumentNullException(parameterName, ErrorMessage(
                    MetadataConstant.Constraints.Parameter.Required.Code(parameterName),
                    MetadataConstant.Constraints.Parameter.Required.Message(parameterName)));
        }
    }

    public static void CollectionNotEmpty<T>(IEnumerable<T>? collection, string parameterName, bool isRequired = MetadataConstant.Constraints.Parameter.CollectionNotEmpty.Value)
    {
        CollectionRequired(collection, parameterName);
        if (isRequired && !collection!.Any())
        {
            throw new ArgumentException(ErrorMessage(
                    MetadataConstant.Constraints.Parameter.CollectionNotEmpty.Code(parameterName),
                    MetadataConstant.Constraints.Parameter.CollectionNotEmpty.Message(parameterName)), parameterName);
        }
    }
    #endregion

    #region Parameters
    public static void ParameterNotNull(object? parameter, string parameterName, bool isRequired = true)
    {
        if (isRequired && parameter is null)
        {
            throw new ArgumentNullException(parameterName, ErrorMessage(
                    MetadataConstant.Constraints.Parameter.Required.Code(parameterName),
                    MetadataConstant.Constraints.Parameter.Required.Message(parameterName)));
        }
    }
    #endregion

    #region Helpers
    public static string ErrorMessage(string code, string message)
    {
        return $"{code}: {message}";
    }
    #endregion
}