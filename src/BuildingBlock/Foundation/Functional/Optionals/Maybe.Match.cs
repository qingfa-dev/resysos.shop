namespace BuildingBlock.Functional.Optionals;

public interface IMaybeMatch<T> : IMaybe<T>
{
    TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Projects whichever state is present.</summary>
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        MaybeGuard.ParameterRequired(some, nameof(some));
        MaybeGuard.ParameterRequired(none, nameof(none));

        return HasValue ? some(Value) : none();
    }
}
