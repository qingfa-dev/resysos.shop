using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Errors;

public readonly partial struct Error
{
    /// <summary>
    /// Maps an exception to a named error. Use this at the boundary of code that cannot avoid
    /// throwing, so callers can stay on the railway instead of catching exceptions.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="code">Optional code override.</param>
    /// <returns>The mapped error.</returns>
    public static Error FromException(Exception exception, string? code = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string message = Describe(exception);

        return exception switch
        {
            OperationCanceledException => Cancelled(
                code ?? ErrorConstant.Default.Cancelled.Code,
                message),
            TimeoutException => Timeout(
                code ?? ErrorConstant.Default.Timeout.Code,
                message),
            NotSupportedException => NotSupported(
                code ?? ErrorConstant.Default.NotSupported.Code,
                message),
            KeyNotFoundException => NotFound(
                code ?? ErrorConstant.Default.NotFound.Code,
                message),
            UnauthorizedAccessException => Unauthorized(
                code ?? ErrorConstant.Default.Unauthorized.Code,
                message),
            ArgumentException => Validation(
                code ?? ErrorConstant.Default.Validation.Code,
                message),
            _ => Unexpected(
                code ?? ErrorConstant.Default.Unexpected.Code,
                message),
        };
    }

    private static string Describe(Exception exception)
        => string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;
}
