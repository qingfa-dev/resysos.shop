using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Errors;

public static class ErrorGuard
{
    #region Helper Methods
    public static string ErrorMessage(string code, string message)
        => $"{code}: {message}";
    #endregion
    #region Code
    public static void CodeRequired(string code)
    {
        var message = ErrorMessage(
            ErrorConstant.Constraints.Code.Required.Code,
            ErrorConstant.Constraints.Code.Required.Message);

        if (code is null)
            throw new ArgumentNullException(nameof(code), message);

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(message, nameof(code));
    }
    public static void CodeMaxLength(string code)
    {
        if (!string.IsNullOrWhiteSpace(code) && code.Length > ErrorConstant.Constraints.Code.MaxLength.Value)
            throw new ArgumentException(ErrorMessage(
                    ErrorConstant.Constraints.Code.MaxLength.Code,
                    ErrorConstant.Constraints.Code.MaxLength.Message));
    }
    public static void CodePattern(string code)
    {
        if (!string.IsNullOrWhiteSpace(code) &&
            !ErrorConstant.Constraints.Code.Pattern.Regex.IsMatch(code))
            throw new ArgumentException(ErrorMessage(
                    ErrorConstant.Constraints.Code.Pattern.Code,
                    ErrorConstant.Constraints.Code.Pattern.Message));
    }
    public static void CodeValid(string code)
    {
        CodeRequired(code);
        CodeMaxLength(code);
        CodePattern(code);
    }
    #endregion

    #region Message
    public static void MessageRequired(string message)
    {
        var error = ErrorMessage(
            ErrorConstant.Constraints.Message.Required.Code,
            ErrorConstant.Constraints.Message.Required.Message);

        if (message is null)
            throw new ArgumentNullException(nameof(message), error);

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(error, nameof(message));
    }
    public static void MessageMaxLength(string message)
    {
        if (!string.IsNullOrWhiteSpace(message) && message.Length > ErrorConstant.Constraints.Message.MaxLength.Value)
            throw new ArgumentException(ErrorMessage(
                    ErrorConstant.Constraints.Message.MaxLength.Code,
                    ErrorConstant.Constraints.Message.MaxLength.Message));
    }
    public static void MessageValid(string message)
    {
        MessageRequired(message);
        MessageMaxLength(message);
    }
    #endregion
    #region Type
    public static void TypeRequired(int type)
    {
        if (type < 0)
            throw new ArgumentOutOfRangeException(nameof(type), ErrorMessage(
                    ErrorConstant.Constraints.Type.Required.Code,
                    ErrorConstant.Constraints.Type.Required.Message));
    }
    public static void TypeOutOfRange(int type)
    {
        if (!Enum.IsDefined(typeof(ErrorType), type))
            throw new ArgumentOutOfRangeException(nameof(type), ErrorMessage(
                    ErrorConstant.Constraints.Type.OutOfRange.Code,
                    ErrorConstant.Constraints.Type.OutOfRange.Message));
    }
    public static void TypeValid(int type)
    {
        TypeRequired(type);
        TypeOutOfRange(type);
    }
    #endregion
}