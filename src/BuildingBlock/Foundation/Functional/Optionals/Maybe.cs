namespace BuildingBlock.Functional.Optionals;

/// <summary>
/// Immutable optional value. The struct is partial so each functional concern
/// can be implemented in its own source file.
/// </summary>
public readonly partial record struct Maybe<T> :
    IMaybeMonad<T>,
    IEnumerable<T>,
    IEquatable<Maybe<T>>
{
    #region Fields

    private readonly T? _value;

    #endregion

    #region Constructor

    private Maybe(T? value)
    {
        _value = value;
        HasValue = value is not null;
    }

    #endregion

    #region Properties

    public bool HasValue { get; }

    public bool HasNoValue => !HasValue;

    public T Value
        => HasValue
            ? _value!
            : throw new DomainException(MaybeConstant.Constraints.Value.Required);

    #endregion

    #region Access

    public bool TryGetValue(out T value)
    {
        value = HasValue ? Value : default!;
        return HasValue;
    }

    public T GetValueOrDefault(T defaultValue)
        => HasValue ? Value : defaultValue;

    /// <summary>Converts the optional into a result, using the supplied error when empty.</summary>
    /// <param name="error">The error to return when there is no value.</param>
    /// <returns>The value, or the supplied error.</returns>
    public Result<T> ToResult(Error error)
        => HasValue ? Result.Success(Value) : Result.Failure<T>(error);

    public void Deconstruct(out bool hasValue, out T value)
    {
        hasValue = HasValue;
        value = HasValue ? Value : default!;
    }

    #endregion

    #region IEnumerable

    public IEnumerator<T> GetEnumerator()
    {
        if (HasValue)
            yield return Value;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    #endregion

    #region Formatting

    public override string ToString()
        => HasValue ? $"Some({_value})" : "None";

    #endregion
}