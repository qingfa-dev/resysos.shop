using System.Diagnostics.CodeAnalysis;

namespace BuildingBlock.Functional.Results;

/// <summary>
/// Terminal conversions: the boundary where a railway result becomes a plain
/// value, an exception, or a boolean. Call these at the edge of the application,
/// after every other step has stayed on the result path.
/// </summary>
public static class ResultTerminalExtensions
{
    #region OrThrow

    /// <summary>
    /// Returns the successful value, or throws the first failure error at the edge.
    /// </summary>
    public static T OrThrow<T>(this IResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? result.Value
            : throw FirstError(result).ToException();
    }

    /// <summary>
    /// Returns the successful value, or throws an exception built from the first error.
    /// </summary>
    public static T OrThrow<T>(this IResult<T> result, Func<Error, Exception> exceptionFactory)
    {
        ArgumentNullException.ThrowIfNull(result);
        ResultGuard.ParameterRequired(exceptionFactory, nameof(exceptionFactory));

        return result.IsSuccess
            ? result.Value
            : throw exceptionFactory(FirstError(result));
    }

    /// <summary>
    /// Returns the successful value, or throws an exception built from every failure error.
    /// </summary>
    public static T OrThrow<T>(this IResult<T> result, Func<IReadOnlyList<Error>, Exception> exceptionFactory)
    {
        ArgumentNullException.ThrowIfNull(result);
        ResultGuard.ParameterRequired(exceptionFactory, nameof(exceptionFactory));

        return result.IsSuccess
            ? result.Value
            : throw exceptionFactory(result.Errors);
    }

    #endregion

    #region ValueOr

    /// <summary>Returns the successful value, or the supplied fallback value.</summary>
    public static T ValueOr<T>(this IResult<T> result, T fallback)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? result.Value
            : fallback;
    }

    /// <summary>Returns the successful value, or a fallback computed from the errors.</summary>
    public static T ValueOr<T>(this IResult<T> result, Func<IReadOnlyList<Error>, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(result);
        ResultGuard.ParameterRequired(fallback, nameof(fallback));

        return result.IsSuccess
            ? result.Value
            : fallback(result.Errors);
    }

    #endregion

    #region TryGetValue

    /// <summary>
    /// Probes the result without throwing: returns the value on success, otherwise the default value.
    /// </summary>
    public static bool TryGetValue<T>(this IResult<T> result, [MaybeNullWhen(false)] out T value)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            value = result.Value;
            return true;
        }

        value = default;
        return false;
    }

    #endregion

    #region ThrowIfFailure

    /// <summary>Throws the first failure error without unwrapping a value.</summary>
    public static void ThrowIfFailure(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            throw FirstError(result).ToException();
    }

    /// <summary>Throws an exception built from the first failure error.</summary>
    public static void ThrowIfFailure(this IResult result, Func<Error, Exception> exceptionFactory)
    {
        ArgumentNullException.ThrowIfNull(result);
        ResultGuard.ParameterRequired(exceptionFactory, nameof(exceptionFactory));

        if (result.IsFailure)
            throw exceptionFactory(FirstError(result));
    }

    #endregion

    private static Error FirstError(IResult result)
        => result.Errors.Count > 0
            ? result.Errors[0]
            : ResultConstant.Constraints.Errors.Empty;
}
