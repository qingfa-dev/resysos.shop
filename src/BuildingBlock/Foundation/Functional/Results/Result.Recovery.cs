namespace BuildingBlock.Functional.Results;

/// <summary>Recovers a failed result either with a value or another result.</summary>
public interface IResultRecovery<T> : IResult<T>
{
    Result<T> Recover(Func<IReadOnlyList<Error>, T> recovery);

    Result<T> RecoverWith(Func<IReadOnlyList<Error>, IResult<T>> recovery);

    ValueTask<Result<T>> RecoverAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<T>> recovery,
        CancellationToken cancellationToken = default);

    ValueTask<Result<T>> RecoverWithAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<IResult<T>>> recovery,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    public Result<T> Recover(Func<IReadOnlyList<Error>, T> recovery)
    {
        ResultGuard.ParameterRequired(recovery, nameof(recovery));

        return IsSuccess
            ? this
            : Success(recovery(Errors));
    }

    public Result<T> RecoverWith(Func<IReadOnlyList<Error>, IResult<T>> recovery)
    {
        ResultGuard.ParameterRequired(recovery, nameof(recovery));

        return IsSuccess
            ? this
            : From(recovery(Errors));
    }

    public async ValueTask<Result<T>> RecoverAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<T>> recovery,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(recovery, nameof(recovery));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccess)
            return this;

        var recovered = await recovery(Errors, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return Success(recovered);
    }

    public async ValueTask<Result<T>> RecoverWithAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<IResult<T>>> recovery,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(recovery, nameof(recovery));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccess)
            return this;

        var recovered = await recovery(Errors, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return From(recovered);
    }
}
