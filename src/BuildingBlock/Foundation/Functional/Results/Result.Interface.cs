namespace BuildingBlock.Functional.Results;

/// <summary>Non-generic success/failure state of a computation.</summary>
public interface IResult
{
    bool IsSuccess { get; }

    bool IsFailure => !IsSuccess;

    /// <summary>Empty on success; contains one or more errors on failure.</summary>
    IReadOnlyList<Error> Errors { get; }
}

/// <summary>A result that carries a value when successful.</summary>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the successful value. Implementations should throw when the result failed.
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Convenient aggregate contract for a full-featured Result value.
/// The mathematical Monad core is Pure/Success plus Bind; the other inherited
/// interfaces provide additional Result capabilities.
/// </summary>
public interface IResultMonad<T> :
    IResultFunctor<T>,
    IResultApplicative<T>,
    IResultBind<T>,
    IResultValidation<T>,
    IResultTap<T>,
    IResultMatch<T>,
    IResultRecovery<T>,
    IResultAlternative<T>,
    IResultRailway<T>,
    IResultErrorMapping<T>
{
}
