namespace BuildingBlock.Functional.Optionals;

/// <summary>Presence state shared by every Maybe value.</summary>
public interface IMaybe
{
    bool HasValue { get; }

    bool HasNoValue => !HasValue;
}

/// <summary>A value that may be present (<c>Some</c>) or absent (<c>None</c>).</summary>
public interface IMaybe<T> : IMaybe
{
    T Value { get; }

    bool TryGetValue(out T value);

    T GetValueOrDefault(T defaultValue);

    void Deconstruct(out bool hasValue, out T value);
}

/// <summary>
/// Full contract for a Maybe value. The mathematical Monad core is
/// <c>Some</c>/<c>Empty</c> plus <c>Bind</c>; the other interfaces add
/// Result-style conveniences.
/// </summary>
public interface IMaybeMonad<T> :
    IMaybe<T>,
    IMaybeFunctor<T>,
    IMaybeBind<T>,
    IMaybeTap<T>,
    IMaybeMatch<T>,
    IMaybeAlternative<T>
{
}
