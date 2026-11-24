namespace BuildingBlock.Functional.Optionals;

public interface IMaybeTap<T> : IMaybe<T>
{
    Maybe<T> Tap(Action<T> action);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Runs a side effect when a value is present and returns the same Maybe.</summary>
    public Maybe<T> Tap(Action<T> action)
    {
        MaybeGuard.ParameterRequired(action, nameof(action));

        if (HasValue)
            action(Value);

        return this;
    }
}
