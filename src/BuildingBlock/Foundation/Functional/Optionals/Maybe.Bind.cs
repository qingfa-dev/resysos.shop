namespace BuildingBlock.Functional.Optionals;

public interface IMaybeBind<T> : IMaybe<T>
{
    Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> binder);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Chains a Maybe-returning function; short-circuits when empty.</summary>
    public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> binder)
    {
        MaybeGuard.ParameterRequired(binder, nameof(binder));

        return HasValue
            ? binder(Value)
            : Maybe<TResult>.Empty();
    }
}
