namespace BuildingBlock.Functional.Guards;

/// <summary>
/// Result-based guard clauses for validating values at the edge of the domain.
/// Every guard returns a <see cref="Result{T}"/> so callers compose validation
/// with <c>Bind</c> instead of catching exceptions.
/// </summary>
public static class Guard
{
    #region Null and empty

    /// <summary>Requires a non-null reference value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The value on success; otherwise a validation failure.</returns>
    public static Result<T> AgainstNull<T>(T? value, Error? error = null)
        where T : class
        => value is null
            ? Result<T>.Failure(error ?? GuardConstant.Null.Error)
            : Result<T>.Success(value);

    /// <summary>Requires a non-empty, non-whitespace string.</summary>
    /// <param name="value">The text to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The text on success; otherwise a validation failure.</returns>
    public static Result<string> AgainstNullOrWhiteSpace(
        string? value,
        Error? error = null)
        => string.IsNullOrWhiteSpace(value)
            ? Result<string>.Failure(error ?? GuardConstant.Empty.Error)
            : Result<string>.Success(value);

    /// <summary>Requires a non-empty collection.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="values">The collection to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The collection on success; otherwise a validation failure.</returns>
    public static Result<IReadOnlyCollection<T>> AgainstEmpty<T>(
        IReadOnlyCollection<T>? values,
        Error? error = null)
        => values is null || values.Count == 0
            ? Result<IReadOnlyCollection<T>>.Failure(
                error ?? GuardConstant.EmptyCollection.Error)
            : Result<IReadOnlyCollection<T>>.Success(values);

    #endregion

    #region Default and ranges

    /// <summary>Requires a value that is not the default for its type.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The value on success; otherwise a validation failure.</returns>
    public static Result<T> AgainstDefault<T>(T value, Error? error = null)
        where T : notnull
        => EqualityComparer<T>.Default.Equals(value, default!)
            ? Result<T>.Failure(error ?? GuardConstant.Default.Error)
            : Result<T>.Success(value);

    /// <summary>Requires a non-negative number.</summary>
    /// <param name="value">The value to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The value on success; otherwise a validation failure.</returns>
    public static Result<int> AgainstNegative(int value, Error? error = null)
        => value < 0
            ? Result<int>.Failure(error ?? GuardConstant.Negative.Error)
            : Result<int>.Success(value);

    /// <summary>Requires a number greater than zero.</summary>
    /// <param name="value">The value to check.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The value on success; otherwise a validation failure.</returns>
    public static Result<int> AgainstZeroOrNegative(
        int value,
        Error? error = null)
        => value <= 0
            ? Result<int>.Failure(error ?? GuardConstant.ZeroOrNegative.Error)
            : Result<int>.Success(value);

    /// <summary>Requires a value within an inclusive range.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The inclusive maximum.</param>
    /// <param name="error">Optional custom error.</param>
    /// <returns>The value on success; otherwise a validation failure.</returns>
    public static Result<T> AgainstOutOfRange<T>(
        T value,
        T min,
        T max,
        Error? error = null)
        where T : IComparable<T>
        => value.CompareTo(min) < 0 || value.CompareTo(max) > 0
            ? Result<T>.Failure(error ?? GuardConstant.OutOfRange.Error)
            : Result<T>.Success(value);

    #endregion

    #region Predicates

    /// <summary>Fails when the supplied condition reports an invalid value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="isInvalid">Whether the value is invalid.</param>
    /// <param name="value">The value to return on success.</param>
    /// <param name="error">The error to return on failure.</param>
    /// <returns>The value on success; otherwise the supplied error.</returns>
    public static Result<T> Against<T>(
        bool isInvalid,
        T value,
        Error error)
        => isInvalid
            ? Result<T>.Failure(error)
            : Result<T>.Success(value);

    /// <summary>Fails when the predicate does not accept the value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="predicate">The acceptance predicate.</param>
    /// <param name="error">The error to return on failure.</param>
    /// <returns>The value on success; otherwise the supplied error.</returns>
    public static Result<T> Ensure<T>(
        T value,
        Func<T, bool> predicate,
        Error error)
        => predicate(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(error);

    #endregion
}
