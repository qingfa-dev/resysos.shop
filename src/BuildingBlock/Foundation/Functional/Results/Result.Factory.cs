using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;

/// <summary>Static construction contract for a concrete result type.</summary>
public interface IResultFactory<TSelf, T>
    where TSelf : IResult<T>
{
    static abstract TSelf Success(T value);

    static abstract TSelf Failure(Error error);

    static abstract TSelf Failure(params Error[] errors);

    static abstract TSelf Failure(IEnumerable<Error> errors);
}

/// <summary>Convenient non-generic factory methods.</summary>
public static partial class Result
{
    public static Result<Unit> Success() => Result<Unit>.Success(Unit.Value);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    public static Result<T> Failure<T>(params Error[] errors) => Result<T>.Failure(errors);

    public static Result<T> Failure<T>(IEnumerable<Error> errors) => Result<T>.Failure(errors);
}

public partial class Result<T>
{
    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error)
    {
        return new Result<T>(new[] { error });
    }

    public static Result<T> Failure(params Error[] errors)
    {
        ResultGuard.ErrorsRequired(errors);
        return new Result<T>(errors);
    }

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        ResultGuard.ErrorsRequired(errors);
        return new Result<T>(errors);
    }

    /// <summary>Converts any IResult&lt;T&gt; into this concrete Result type.</summary>
    public static Result<T> From(IResult<T> result)
    {
        ResultGuard.ParameterRequired(result, nameof(result));

        return result.IsSuccess
            ? Success(result.Value)
            : Failure(result.Errors);
    }

    public static implicit operator Result<T>(T value)
      => Success(value);

    public static implicit operator Result<T>(Error error)
        => Failure(error);

    public static implicit operator Result<T>(Error[] errors)
        => Failure(errors);

    public static implicit operator Result<T>(List<Error> errors)
        => Failure(errors);

}