namespace BuildingBlock.Specifications.InMemory;

/// <summary>Default in-memory evaluator: criteria, ordering and paging.</summary>
public class InMemorySpecificationEvaluator : IInMemorySpecificationEvaluator
{
    #region Construction

    /// <summary>
    /// Protected so instances can only be created through <see cref="Create"/>.
    /// </summary>
    protected InMemorySpecificationEvaluator()
    {
    }

    #endregion

    #region Factory

    /// <summary>Creates an evaluator.</summary>
    public static Result<InMemorySpecificationEvaluator> Create()
        => Result.Success(new InMemorySpecificationEvaluator());

    #endregion

    #region Evaluation

    /// <inheritdoc />
    public Result<IEnumerable<T>> Apply<T>(
        IEnumerable<T> source,
        ISpecification<T> specification)
        => InMemorySpecificationGuard.SourceRequired(source)
            .Bind(validSource => InMemorySpecificationGuard
                .SpecificationRequired(specification)
                .Map(validSpec => ApplyCore(validSource, validSpec)));

    #endregion

    #region Composition

    private static IEnumerable<T> ApplyCore<T>(
        IEnumerable<T> source,
        ISpecification<T> specification)
    {
        var predicate = specification.Criteria.Compile();
        IEnumerable<T> result = source.Where(predicate);

        IOrderedEnumerable<T>? ordered = null;
        foreach ((Expression<Func<T, object>> keySelector, bool descending) in specification.OrderBy)
        {
            var key = keySelector.Compile();
            ordered = ordered is null
                ? descending
                    ? result.OrderByDescending(key)
                    : result.OrderBy(key)
                : descending
                    ? ordered.ThenByDescending(key)
                    : ordered.ThenBy(key);
        }

        if (ordered is not null)
        {
            result = ordered;
        }

        if (!specification.IsPagingEnabled)
        {
            return result;
        }

        result = result.Skip(specification.Skip ?? 0);
        return specification.Take is { } take ? result.Take(take) : result;
    }

    #endregion
}
