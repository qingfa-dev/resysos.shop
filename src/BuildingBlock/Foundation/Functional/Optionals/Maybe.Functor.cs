namespace BuildingBlock.Functional.Optionals;

public interface IMaybeFunctor<T> : IMaybe<T>
{
    Maybe<TResult> Map<TResult>(Func<T, TResult> mapper);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Maps the value when present; returns Empty when absent.</summary>
    public Maybe<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        MaybeGuard.ParameterRequired(mapper, nameof(mapper));

        return HasValue
            ? Maybe<TResult>.FromNullable(mapper(Value))
            : Maybe<TResult>.Empty();
    }
}
