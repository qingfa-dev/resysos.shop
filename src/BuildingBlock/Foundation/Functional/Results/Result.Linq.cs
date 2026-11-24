using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;

/// <summary>LINQ query-syntax support for Result.</summary>
public static class ResultLinqExtensions
{
    public static Result<TResult> Select<T, TResult>(
        this Result<T> result,
        Func<T, TResult> selector)
    {
        ResultGuard.ParameterRequired(result, nameof(result));
        return result.Map(selector);
    }

    /// <summary>
    /// Enables <c>where</c> in query syntax. Filtering a successful result with an
    /// unsatisfied predicate turns it into a failure using
    /// <see cref="ResultConstant.Constraints.Filter.Unsatisfied"/>.
    /// </summary>
    public static Result<T> Where<T>(this Result<T> result, Func<T, bool> predicate)
        => Where(result, predicate, ResultConstant.Constraints.Filter.Unsatisfied);

    public static Result<T> Where<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        ResultGuard.ParameterRequired(result, nameof(result));
        ResultGuard.ParameterRequired(predicate, nameof(predicate));

        return result.Ensure(predicate, error);
    }

    public static Result<TResult> SelectMany<T, TIntermediate, TResult>(
        this Result<T> result,
        Func<T, IResult<TIntermediate>> binder,
        Func<T, TIntermediate, TResult> projector)
    {
        ResultGuard.ParameterRequired(result, nameof(result));
        ResultGuard.ParameterRequired(binder, nameof(binder));
        ResultGuard.ParameterRequired(projector, nameof(projector));

        return result.Bind(value =>
            Result<TIntermediate>.From(binder(value))
                .Map(intermediate => projector(value, intermediate)));
    }
}
