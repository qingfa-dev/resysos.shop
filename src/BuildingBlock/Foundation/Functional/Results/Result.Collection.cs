using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;

namespace BuildingBlock.Functional.Results;


/// <summary>Operations over collections of result values.</summary>
public interface IResultCollection
{
    Result<Unit> Combine(IEnumerable<IResult> results);

    Result<IReadOnlyList<T>> Sequence<T>(IEnumerable<IResult<T>> results);

    Result<IReadOnlyList<TResult>> Traverse<T, TResult>(
        IEnumerable<T> source,
        Func<T, IResult<TResult>> mapper);
}

/// <summary>
/// Collection-level Result operations. Sequence and Combine accumulate all
/// errors rather than short-circuiting at the first failed item.
/// </summary>
public sealed class ResultCollection : IResultCollection
{
    public Result<Unit> Combine(IEnumerable<IResult> results)
    {
        ResultGuard.ParameterRequired(results, nameof(results));

        var errors = new List<Error>();

        foreach (var result in results)
        {
            ResultGuard.ParameterRequired(result, nameof(result));

            if (result.IsFailure)
                errors.AddRange(result.Errors);
        }

        return errors.Count == 0
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure(errors);
    }

    public Result<IReadOnlyList<T>> Sequence<T>(IEnumerable<IResult<T>> results)
    {
        ResultGuard.ParameterRequired(results, nameof(results));

        var values = new List<T>();
        var errors = new List<Error>();

        foreach (var result in results)
        {
            ResultGuard.ParameterRequired(result, nameof(result));

            if (result.IsFailure)
                errors.AddRange(result.Errors);
            else
                values.Add(result.Value);
        }

        return errors.Count == 0
            ? Result<IReadOnlyList<T>>.Success(values.AsReadOnly())
            : Result<IReadOnlyList<T>>.Failure(errors);
    }

    public Result<IReadOnlyList<TResult>> Traverse<T, TResult>(
        IEnumerable<T> source,
        Func<T, IResult<TResult>> mapper)
    {
        ResultGuard.ParameterRequired(source, nameof(source));
        ResultGuard.ParameterRequired(mapper, nameof(mapper));

        return Sequence(source.Select(mapper));
    }
}
