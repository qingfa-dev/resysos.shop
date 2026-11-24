namespace BuildingBlock.Functional.Results;

/// <summary>Side-effect hooks that do not change the result state or value.</summary>
public interface IResultTap<T> : IResult<T>
{
    Result<T> Tap(Action<T> action);

    Result<T> TapError(Action<IReadOnlyList<Error>> action);

    ValueTask<Result<T>> TapAsync(
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default);

    ValueTask<Result<T>> TapErrorAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    public Result<T> Tap(Action<T> action)
    {
        ResultGuard.ParameterRequired(action, nameof(action));

        if (IsSuccess)
            action(Value);

        return this;
    }

    public Result<T> TapError(Action<IReadOnlyList<Error>> action)
    {
        ResultGuard.ParameterRequired(action, nameof(action));

        if (IsFailure)
            action(Errors);

        return this;
    }

    public async ValueTask<Result<T>> TapAsync(
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(action, nameof(action));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccess)
        {
            await action(Value, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return this;
    }

    public async ValueTask<Result<T>> TapErrorAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(action, nameof(action));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsFailure)
        {
            await action(Errors, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return this;
    }
}
