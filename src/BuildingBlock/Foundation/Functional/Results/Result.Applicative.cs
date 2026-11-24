using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;

/// <summary>
/// Applicative operations. Unlike Bind, these operations combine independent
/// computations and accumulate errors from both inputs.
/// </summary>
public interface IResultApplicative<T> : IResult<T>
{
    /// <summary>Applies a successful function result to this successful value.</summary>
    Result<TResult> Apply<TResult>(IResult<Func<T, TResult>> functionResult);

    /// <summary>Combines this result and a second independent result.</summary>
    Result<TResult> Lift2<TSecond, TResult>(
        IResult<TSecond> second,
        Func<T, TSecond, TResult> mapper);
}

public partial class Result<T>
{
    /// <summary>
    /// Applies a function held in another Result to this value. If both sides
    /// fail, errors are accumulated in value-result then function-result order.
    /// </summary>
    public Result<TResult> Apply<TResult>(IResult<Func<T, TResult>> functionResult)
    {
        ResultGuard.ParameterRequired(functionResult, nameof(functionResult));

        if (IsFailure && functionResult.IsFailure)
            return Result<TResult>.Failure(Errors.Concat(functionResult.Errors));

        if (IsFailure)
            return Result<TResult>.Failure(Errors);

        if (functionResult.IsFailure)
            return Result<TResult>.Failure(functionResult.Errors);

        var function = functionResult.Value;
        ResultGuard.ParameterRequired(function, nameof(function));

        return Result<TResult>.Success(function(Value));
    }

    /// <summary>
    /// Combines two independent successful values. Errors from both failed
    /// inputs are accumulated in input order.
    /// </summary>
    public Result<TResult> Lift2<TSecond, TResult>(
        IResult<TSecond> second,
        Func<T, TSecond, TResult> mapper)
    {
        ResultGuard.ParameterRequired(second, nameof(second));
        ResultGuard.ParameterRequired(mapper, nameof(mapper));

        if (IsFailure && second.IsFailure)
            return Result<TResult>.Failure(Errors.Concat(second.Errors));

        if (IsFailure)
            return Result<TResult>.Failure(Errors);

        if (second.IsFailure)
            return Result<TResult>.Failure(second.Errors);

        return Result<TResult>.Success(mapper(Value, second.Value));
    }
}
