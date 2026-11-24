namespace BuildingBlock.Functional.Optionals;

/// <summary>LINQ query-syntax support for Maybe.</summary>
public static class MaybeLinqExtensions
{
    public static Maybe<TResult> Select<T, TResult>(
        this Maybe<T> source,
        Func<T, TResult> selector)
        => source.Map(selector);

    public static Maybe<TResult> SelectMany<T, TResult>(
        this Maybe<T> source,
        Func<T, Maybe<TResult>> binder)
        => source.Bind(binder);

    public static Maybe<TOutput> SelectMany<T, TIntermediate, TOutput>(
        this Maybe<T> source,
        Func<T, Maybe<TIntermediate>> binder,
        Func<T, TIntermediate, TOutput> projector)
    {
        MaybeGuard.ParameterRequired(binder, nameof(binder));
        MaybeGuard.ParameterRequired(projector, nameof(projector));

        if (!source.HasValue)
            return Maybe<TOutput>.Empty();

        var value = source.Value;
        return binder(value).Map(intermediate => projector(value, intermediate));
    }

    public static Maybe<T> Where<T>(
        this Maybe<T> source,
        Func<T, bool> predicate)
    {
        MaybeGuard.ParameterRequired(predicate, nameof(predicate));

        return source.HasValue && predicate(source.Value)
            ? source
            : Maybe<T>.Empty();
    }
}
