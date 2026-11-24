using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;

/// <summary>
/// Monadic join: collapses a nested <see cref="Result{T}"/> produced by a binder.
/// <c>Flatten</c> is <c>Bind(identity)</c>; it keeps chained results flat.
/// </summary>
public static class ResultFlattenExtensions
{
    public static Result<T> Flatten<T>(this Result<Result<T>> nested)
    {
        ResultGuard.ParameterRequired(nested, nameof(nested));

        return nested.IsSuccess
            ? nested.Value
            : Result<T>.Failure(nested.Errors);
    }

    public static Result<T> Flatten<T>(this Result<IResult<T>> nested)
    {
        ResultGuard.ParameterRequired(nested, nameof(nested));

        return nested.IsSuccess
            ? Result<T>.From(nested.Value)
            : Result<T>.Failure(nested.Errors);
    }

    /// <summary>Alias of <see cref="Flatten{T}(Result{Result{T}})"/>.</summary>
    public static Result<T> Join<T>(this Result<Result<T>> nested) => nested.Flatten();
}
