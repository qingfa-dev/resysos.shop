using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;

/// <summary>Conditional validation of a successful value.</summary>
public interface IResultValidation<T> : IResult<T>
{
    Result<T> Ensure(Func<T, bool> predicate, Error error);

    Result<T> Ensure(Func<T, bool> predicate, Func<T, Error> errorFactory);

    ValueTask<Result<T>> EnsureAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        Func<T, Error> errorFactory,
        CancellationToken cancellationToken = default);
}

public partial class Result<T>
{
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        return Ensure(predicate, _ => error);
    }

    public Result<T> Ensure(Func<T, bool> predicate, Func<T, Error> errorFactory)
    {
        ResultGuard.ParameterRequired(predicate, nameof(predicate));
        ResultGuard.ParameterRequired(errorFactory, nameof(errorFactory));

        if (IsFailure)
            return this;

        if (predicate(Value))
            return this;

        return Failure(errorFactory(Value));
    }

    public async ValueTask<Result<T>> EnsureAsync(
        Func<T, CancellationToken, ValueTask<bool>> predicate,
        Func<T, Error> errorFactory,
        CancellationToken cancellationToken = default)
    {
        ResultGuard.ParameterRequired(predicate, nameof(predicate));
        ResultGuard.ParameterRequired(errorFactory, nameof(errorFactory));
        cancellationToken.ThrowIfCancellationRequested();

        if (IsFailure)
            return this;

        var isValid = await predicate(Value, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return isValid
            ? this
            : Failure(errorFactory(Value));
    }
}
