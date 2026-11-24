namespace BuildingBlock.Functional.Results;

/// <summary>
/// Alternative (monadic choice): returns this result when successful, otherwise
/// evaluates a fallback. This is the fail-fast counterpart of <c>Apply</c>,
/// which accumulates errors instead of choosing the first success.
/// </summary>
public interface IResultAlternative<T> : IResult<T>
{
    Result<T> OrElse(IResult<T> fallback);

    Result<T> OrElse(Func<IReadOnlyList<Error>, IResult<T>> fallback);

    ValueTask<Result<T>> OrElseAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<IResult<T>>> fallback,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    /// <summary>Returns this result on success, otherwise the fallback result.</summary>
    public Result<T> OrElse(IResult<T> fallback)
    {
        ResultGuard.ParameterRequired(fallback, nameof(fallback));

        return IsSuccess
            ? this
            : From(fallback);
    }

    /// <summary>Returns this result on success, otherwise builds a fallback from the errors.</summary>
    public Result<T> OrElse(Func<IReadOnlyList<Error>, IResult<T>> fallback)
    {
        ResultGuard.ParameterRequired(fallback, nameof(fallback));

        return IsSuccess
            ? this
            : From(fallback(Errors));
    }

    public async ValueTask<Result<T>> OrElseAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<IResult<T>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(fallback, nameof(fallback));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccess)
            return this;

        var next = await fallback(Errors, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return From(next);
    }
}
