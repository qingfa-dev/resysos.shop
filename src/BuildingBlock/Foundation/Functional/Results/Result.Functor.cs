namespace BuildingBlock.Functional.Results;

/// <summary>Functor operations: transform a successful value.</summary>
public interface IResultFunctor<T> : IResult<T>
{
    Result<TResult> Map<TResult>(Func<T, TResult> mapper);

    ValueTask<Result<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> mapper,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    /// <summary>Maps the successful value; failures pass through unchanged.</summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        ResultGuard.ParameterRequired(mapper, nameof(mapper));

        return IsFailure
            ? Result<TResult>.Failure(Errors)
            : Result<TResult>.Success(mapper(Value));
    }

    public async ValueTask<Result<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> mapper,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(mapper, nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsFailure)
            return Result<TResult>.Failure(Errors);

        var mapped = await mapper(Value, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Result<TResult>.Success(mapped);
    }
}
