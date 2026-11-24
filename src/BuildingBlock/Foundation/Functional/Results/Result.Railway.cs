using BuildingBlock.Functional.Errors;

namespace BuildingBlock.Functional.Results;

/// <summary>
/// Railway-oriented naming for the two tracks. <c>Then</c> continues the success
/// track (alias of <c>Bind</c>); <c>OnSuccess</c>/<c>OnFailure</c> run a side
/// effect on the matching track (aliases of <c>Tap</c>/<c>TapError</c>).
/// </summary>
public interface IResultRailway<T> : IResult<T>
{
    Result<TResult> Then<TResult>(Func<T, IResult<TResult>> next);

    ValueTask<Result<TResult>> ThenAsync<TResult>(
        Func<T, CancellationToken, ValueTask<IResult<TResult>>> next,
        CancellationToken cancellationToken = default);

    Result<T> OnSuccess(Action<T> action);

    ValueTask<Result<T>> OnSuccessAsync(
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default);

    Result<T> OnFailure(Action<IReadOnlyList<Error>> action);

    ValueTask<Result<T>> OnFailureAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    public Result<TResult> Then<TResult>(Func<T, IResult<TResult>> next) => Bind(next);

    public ValueTask<Result<TResult>> ThenAsync<TResult>(
        Func<T, CancellationToken, ValueTask<IResult<TResult>>> next,
        CancellationToken cancellationToken = default) => BindAsync(next, cancellationToken);

    public Result<T> OnSuccess(Action<T> action) => Tap(action);

    public ValueTask<Result<T>> OnSuccessAsync(
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default) => TapAsync(action, cancellationToken);

    public Result<T> OnFailure(Action<IReadOnlyList<Error>> action) => TapError(action);

    public ValueTask<Result<T>> OnFailureAsync(
        Func<IReadOnlyList<Error>, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default) => TapErrorAsync(action, cancellationToken);
}
