using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Metadata;

namespace BuildingBlock.Functional.Errors;

/// <summary>
/// Immutable, validated error value. Identity is defined by
/// <see cref="Type"/>, <see cref="Code"/>, and <see cref="Message"/>;
/// <see cref="Metadata"/> is diagnostic and deliberately excluded from equality.
/// </summary>
public readonly partial struct Error : IError, IEquatable<Error>
{
    #region Constructors

    private Error(string code, string message, ErrorType type)
    {
        ErrorGuard.TypeValid((int)type);
        ErrorGuard.CodeValid(code);
        ErrorGuard.MessageValid(message);

        Code = code;
        Message = message;
        Type = type;
    }

    #endregion

    #region Properties

    public ErrorType Type { get; private init; }

    public string Code { get; private init; }

    public string Message { get; private init; }

    public IMetadata Metadata { get; private init; } = DefaultMetadata.Empty();

    #endregion

    #region Factory

    public static Error Create(
        string code = ErrorConstant.Default.Code,
        string message = ErrorConstant.Default.Message,
        ErrorType type = ErrorConstant.Default.Type)
        => new(code, message, type);

    #region By Types

    public static Error Unexpected(
        string code = ErrorConstant.Default.Unexpected.Code,
        string message = ErrorConstant.Default.Unexpected.Message)
        => Create(code, message, ErrorConstant.Default.Unexpected.Type);

    public static Error Validation(
        string code = ErrorConstant.Default.Validation.Code,
        string message = ErrorConstant.Default.Validation.Message)
        => Create(code, message, ErrorConstant.Default.Validation.Type);

    public static Error NotFound(
        string code = ErrorConstant.Default.NotFound.Code,
        string message = ErrorConstant.Default.NotFound.Message)
        => Create(code, message, ErrorConstant.Default.NotFound.Type);

    public static Error Conflict(
        string code = ErrorConstant.Default.Conflict.Code,
        string message = ErrorConstant.Default.Conflict.Message)
        => Create(code, message, ErrorConstant.Default.Conflict.Type);

    public static Error Unauthorized(
        string code = ErrorConstant.Default.Unauthorized.Code,
        string message = ErrorConstant.Default.Unauthorized.Message)
        => Create(code, message, ErrorConstant.Default.Unauthorized.Type);

    public static Error Forbidden(
        string code = ErrorConstant.Default.Forbidden.Code,
        string message = ErrorConstant.Default.Forbidden.Message)
        => Create(code, message, ErrorConstant.Default.Forbidden.Type);

    public static Error BusinessRule(
        string code = ErrorConstant.Default.BusinessRule.Code,
        string message = ErrorConstant.Default.BusinessRule.Message)
        => Create(code, message, ErrorConstant.Default.BusinessRule.Type);

    public static Error Dependency(
        string code = ErrorConstant.Default.Dependency.Code,
        string message = ErrorConstant.Default.Dependency.Message)
        => Create(code, message, ErrorConstant.Default.Dependency.Type);

    public static Error Timeout(
        string code = ErrorConstant.Default.Timeout.Code,
        string message = ErrorConstant.Default.Timeout.Message)
        => Create(code, message, ErrorConstant.Default.Timeout.Type);

    public static Error RateLimited(
        string code = ErrorConstant.Default.RateLimited.Code,
        string message = ErrorConstant.Default.RateLimited.Message)
        => Create(code, message, ErrorConstant.Default.RateLimited.Type);

    public static Error Cancelled(
        string code = ErrorConstant.Default.Cancelled.Code,
        string message = ErrorConstant.Default.Cancelled.Message)
        => Create(code, message, ErrorConstant.Default.Cancelled.Type);

    public static Error NotSupported(
        string code = ErrorConstant.Default.NotSupported.Code,
        string message = ErrorConstant.Default.NotSupported.Message)
        => Create(code, message, ErrorConstant.Default.NotSupported.Type);

    public static Error InvalidState(
        string code = ErrorConstant.Default.InvalidState.Code,
        string message = ErrorConstant.Default.InvalidState.Message)
        => Create(code, message, ErrorConstant.Default.InvalidState.Type);

    #endregion

    #endregion

    #region Equality

    public bool Equals(Error other)
        => Type == other.Type
        && string.Equals(Code, other.Code, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Error other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Type, Code, Message);

    public static bool operator ==(Error left, Error right) => left.Equals(right);

    public static bool operator !=(Error left, Error right) => !left.Equals(right);

    #endregion

    #region Conversions

    public override string ToString()
        => $"Error: Code={Code}, Message={Message}, Type={Type}";

    public Exception ToException() => new(ToString());

    #endregion

    #region Methods

    #region Overrides

    public Error WithCode(string code)
    {
        ErrorGuard.CodeValid(code);
        return this with { Code = code };
    }

    public Error WithMessage(string message)
    {
        ErrorGuard.MessageValid(message);
        return this with { Message = message };
    }

    public Error WithType(ErrorType type)
    {
        ErrorGuard.TypeValid((int)type);
        return this with { Type = type };
    }

    #endregion

    #region Metadata

    public Error WithMetadata(string key, object value)
    {
        var metadata = Metadata is { Count: > 0 }
            ? DefaultMetadata.From(Metadata)
            : DefaultMetadata.Empty();

        metadata.WithMetadata(key, value);
        return this with { Metadata = metadata };
    }

    private bool TryGetMetadata<T>(string key, out T? value)
    {
        if (Metadata is null)
        {
            value = default;
            return false;
        }

        return Metadata.TryGetMetadata(key, out value);
    }

    #region Wellknown metadata

    public Error WithTraceId(Guid traceId)
        => WithMetadata(ErrorConstant.Metadata.TraceId, traceId);

    public Guid? GetTraceId()
    {
        if (TryGetMetadata<Guid>(ErrorConstant.Metadata.TraceId, out var traceId))
            return traceId;

        if (TryGetMetadata<string>(ErrorConstant.Metadata.TraceId, out var text)
            && Guid.TryParse(text, out var parsed))
            return parsed;

        return null;
    }

    public Error WithTimestamp(DateTimeOffset timestamp)
        => WithMetadata(ErrorConstant.Metadata.Timestamp, timestamp);

    public DateTimeOffset? GetTimestamp()
        => TryGetMetadata<DateTimeOffset>(ErrorConstant.Metadata.Timestamp, out var timestamp)
            ? timestamp
            : null;

    public Error WithResource(string resource)
        => WithMetadata(ErrorConstant.Metadata.Resource, resource);

    public string? GetResource()
        => TryGetMetadata<string>(ErrorConstant.Metadata.Resource, out var resource)
            ? resource
            : null;

    public Error WithField(string field)
        => WithMetadata(ErrorConstant.Metadata.Field, field);

    public string? GetField()
    {
        if (TryGetMetadata<string>(ErrorConstant.Metadata.Field, out var field)
            && !string.IsNullOrEmpty(field))
            return field;

        return null;
    }

    public Error WithAttempt(int attempt)
        => WithMetadata(ErrorConstant.Metadata.Attempt, attempt);

    public int? GetAttempt()
        => TryGetMetadata<int>(ErrorConstant.Metadata.Attempt, out var attempt)
            ? attempt
            : null;

    public Error WithExceptionType(Type exceptionType)
        => WithMetadata(ErrorConstant.Metadata.ExceptionType, exceptionType);

    public Type? GetExceptionType()
        => TryGetMetadata<Type>(ErrorConstant.Metadata.ExceptionType, out var exceptionType)
            ? exceptionType
            : null;

    public Error WithExceptionMessage(string exceptionMessage)
        => WithMetadata(ErrorConstant.Metadata.ExceptionMessage, exceptionMessage);

    public string? GetExceptionMessage()
    {
        if (TryGetMetadata<string>(ErrorConstant.Metadata.ExceptionMessage, out var exceptionMessage)
            && !string.IsNullOrEmpty(exceptionMessage))
            return exceptionMessage;

        return null;
    }

    public Error WithException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var error = WithExceptionType(exception.GetType())
            .WithExceptionMessage(exception.Message);

        if (exception.InnerException is not null)
            error = error.WithMetadata(ErrorConstant.Metadata.InnerException, exception.InnerException);

        return error;
    }

    public Exception? GetInnerException()
        => TryGetMetadata<Exception>(ErrorConstant.Metadata.InnerException, out var innerException)
            ? innerException
            : null;

    #endregion

    #endregion

    #endregion
}
