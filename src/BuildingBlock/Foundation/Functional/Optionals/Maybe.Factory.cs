namespace BuildingBlock.Functional.Optionals;
/// <summary>Static construction contract for a concrete Maybe type.</summary>
public interface IMaybeFactory<out TSelf, T>
    where TSelf : IMaybe<T>
{
    static abstract TSelf Some(T value);

    static abstract TSelf Empty();

    static abstract TSelf FromNullable(T? value);
}

/// <summary>Convenient non-generic factory methods.</summary>
public static class Maybe
{
    public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);

    public static Maybe<T> Empty<T>() => Maybe<T>.Empty();

    public static Maybe<T> FromNullable<T>(T? value) => Maybe<T>.FromNullable(value);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Creates a present value. Use <see cref="Empty"/> or <see cref="FromNullable"/> for null.</summary>
    public static Maybe<T> Some(T value)
    {
        MaybeGuard.ValueRequired(value);
        return new(value);
    }

    public static Maybe<T> Empty() => default;

    public static Maybe<T> FromNullable(T? value)
        => value is null ? Empty() : Some(value);

    public static implicit operator Maybe<T>(T? value)
        => FromNullable(value);
}