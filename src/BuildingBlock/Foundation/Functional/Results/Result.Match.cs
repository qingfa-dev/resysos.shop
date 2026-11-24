namespace BuildingBlock.Functional.Results;

/// <summary>Consumes both possible states of a result.</summary>
public interface IResultMatch<T> : IResult<T>
{
    TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure);

    void Switch(
        Action<T> onSuccess,
        Action<IReadOnlyList<Error>> onFailure);

    ValueTask<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> onSuccess,
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<TResult>> onFailure,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        ResultGuard.ParameterRequired(onSuccess, nameof(onSuccess));
        ResultGuard.ParameterRequired(onFailure, nameof(onFailure));

        return IsSuccess
            ? onSuccess(Value)
            : onFailure(Errors);
    }

    public void Switch(
        Action<T> onSuccess,
        Action<IReadOnlyList<Error>> onFailure)
    {
        ResultGuard.ParameterRequired(onSuccess, nameof(onSuccess));
        ResultGuard.ParameterRequired(onFailure, nameof(onFailure));

        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Errors);
    }

    public async ValueTask<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> onSuccess,
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask<TResult>> onFailure,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(onSuccess, nameof(onSuccess));
        ResultGuard.ParameterRequired(onFailure, nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();

        var result = IsSuccess
            ? await onSuccess(Value, cancellationToken).ConfigureAwait(false)
            : await onFailure(Errors, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
