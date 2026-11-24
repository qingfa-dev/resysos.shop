using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Results;

/// <summary>
/// Bridges code that throws into the railway. Prefer explicit failures, but use these helpers at
/// the boundary of framework or third-party calls so expected failures never escape as exceptions.
/// </summary>
public static partial class Result
{
    /// <summary>Runs a function and captures any exception as a failure.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="operation">The operation to run.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>The value, or the mapped error.</returns>
    public static Result<T> Try<T>(
        Func<T> operation,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            return Success(operation());
        }
#pragma warning disable CA1031 // The whole point is to capture the failure as a result.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            return Failure<T>(Map(exception, mapException));
        }
    }

    /// <summary>Runs an action and captures any exception as a failure.</summary>
    /// <param name="operation">The operation to run.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>Unit on success, or the mapped error.</returns>
    public static Result<Unit> Try(
        Action operation,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return Try(
            () =>
            {
                operation();
                return Unit.Value;
            },
            mapException);
    }

    /// <summary>Runs an asynchronous function and captures any exception as a failure.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="operation">The operation to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>The value, or the mapped error.</returns>
    public static async ValueTask<Result<T>> TryAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            return Success(await operation(cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure<T>(Error.Cancelled());
        }
#pragma warning disable CA1031 // The whole point is to capture the failure as a result.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            return Failure<T>(Map(exception, mapException));
        }
    }

    /// <summary>Runs an asynchronous action and captures any exception as a failure.</summary>
    /// <param name="operation">The operation to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>Unit on success, or the mapped error.</returns>
    public static ValueTask<Result<Unit>> TryAsync(
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return TryAsync(
            async token =>
            {
                await operation(token).ConfigureAwait(false);
                return Unit.Value;
            },
            cancellationToken,
            mapException);
    }

    /// <summary>Turns a nullable reference into a result, using the supplied error when null.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The possibly missing value.</param>
    /// <param name="error">The error to return when the value is null.</param>
    /// <returns>The value, or the supplied error.</returns>
    public static Result<T> FromNullable<T>(T? value, Error error)
        where T : class
        => value is null ? Failure<T>(error) : Success(value);

    /// <summary>Creates a failure with the default not-found error for a type.</summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="code">Optional code override.</param>
    /// <param name="message">Optional message override.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> NotFound<T>(string? code = null, string? message = null)
        => Failure<T>(Error.NotFound(
            code ?? "result.not_found",
            message ?? $"{typeof(T).Name} was not found."));

    /// <summary>Wraps a result so a success value becomes <see cref="Unit"/>.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to discard.</param>
    /// <returns>Unit on success, or the same errors.</returns>
    public static Result<Unit> ToUnit<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Success()
            : Failure<Unit>(result.Errors);
    }

    private static Error Map(Exception exception, Func<Exception, Error>? mapException)
        => mapException is null ? Error.FromException(exception) : mapException(exception);
}

/// <summary>Railway helpers for nullable and boolean checks.</summary>
public static class ResultExtensions
{
    /// <summary>Fails when the value does not satisfy the predicate.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="predicate">The condition a successful value must satisfy.</param>
    /// <param name="error">The error to return when the condition fails.</param>
    /// <returns>The same result, or the supplied error.</returns>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        return result.IsFailure || predicate(result.Value)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>Fails when an asynchronous condition rejects the value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="predicate">The condition a successful value must satisfy.</param>
    /// <param name="error">The error to return when the condition fails.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The same result, or the supplied error.</returns>
    public static async ValueTask<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value, cancellationToken).ConfigureAwait(false)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>Converts a task into a result, capturing any exception as a failure.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>The value, or the mapped error.</returns>
    public static async ValueTask<Result<T>> AsResultAsync<T>(
        this Task<T> task,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        return await Result.TryAsync(
            async _ => await task.ConfigureAwait(false),
            CancellationToken.None,
            mapException).ConfigureAwait(false);
    }

    /// <summary>Converts a task into a result, capturing any exception as a failure.</summary>
    /// <param name="task">The task to await.</param>
    /// <param name="mapException">Optional exception-to-error mapping.</param>
    /// <returns>Unit on success, or the mapped error.</returns>
    public static async ValueTask<Result<Unit>> AsResultAsync(
        this Task task,
        Func<Exception, Error>? mapException = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        return await Result.TryAsync(
            async _ =>
            {
                await task.ConfigureAwait(false);
                return Unit.Value;
            },
            CancellationToken.None,
            mapException).ConfigureAwait(false);
    }
}
