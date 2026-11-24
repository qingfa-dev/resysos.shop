namespace BuildingBlock.Functional.Results;

/// <summary>Monad bind/flatMap operation: chain a result-producing function.</summary>
public interface IResultBind<T> : IResult<T>
{
    Result<TResult> Bind<TResult>(Func<T, IResult<TResult>> binder);

    ValueTask<Result<TResult>> BindAsync<TResult>(
        Func<T, CancellationToken, ValueTask<IResult<TResult>>> binder,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    /// <summary>
    /// Chains a result-producing function. This is fail-fast: the binder is not
    /// invoked when this result is already a failure.
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, IResult<TResult>> binder)
    {
        ResultGuard.ParameterRequired(binder, nameof(binder));

        if (IsFailure)
            return Result<TResult>.Failure(Errors);

        return Result<TResult>.From(binder(Value));
    }

    public async ValueTask<Result<TResult>> BindAsync<TResult>(
        Func<T, CancellationToken, ValueTask<IResult<TResult>>> binder,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(binder, nameof(binder));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsFailure)
            return Result<TResult>.Failure(Errors);

        var next = await binder(Value, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Result<TResult>.From(next);
    }
}
