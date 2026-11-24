namespace BuildingBlock.Functional.Results;

/// <summary>Extension syntax for collection-level Result operations.</summary>
public static class ResultCollectionExtensions
{
    private static readonly ResultCollection Collection = new();

    public static Result<IReadOnlyList<T>> Sequence<T>(
        this IEnumerable<IResult<T>> results)
        => Collection.Sequence(results);

    public static Result<IReadOnlyList<TResult>> Traverse<T, TResult>(
        this IEnumerable<T> source,
        Func<T, IResult<TResult>> mapper)
        => Collection.Traverse(source, mapper);

    public static Result<Unit> CombineResults(this IEnumerable<IResult> results)
        => Collection.Combine(results);

    public static Result<Unit> CombineResults(params IResult[] results)
    {
        ResultGuard.ParameterRequired(results, nameof(results));
        return Collection.Combine(results);
    }
}
