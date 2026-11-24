namespace BuildingBlock.Functional.Optionals;

public interface IMaybeAlternative<T> : IMaybe<T>
{
    Maybe<T> Or(Maybe<T> alternative);

    Maybe<T> OrElse(Func<Maybe<T>> alternativeFactory);
}

public readonly partial record struct Maybe<T>
{
    /// <summary>Returns this value when present, otherwise the alternative.</summary>
    public Maybe<T> Or(Maybe<T> alternative)
        => HasValue ? this : alternative;

    /// <summary>Returns this value when present, otherwise invokes the factory.</summary>
    public Maybe<T> OrElse(Func<Maybe<T>> alternativeFactory)
    {
        MaybeGuard.ParameterRequired(alternativeFactory, nameof(alternativeFactory));

        return HasValue ? this : alternativeFactory();
    }
}
