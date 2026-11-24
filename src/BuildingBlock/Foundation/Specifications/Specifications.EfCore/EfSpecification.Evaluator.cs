using System.Linq.Expressions;
using Microsoft.Extensions.Options;


namespace BuildingBlock.Specifications.EfCore;

/// <summary>Default EF Core evaluator: criteria, includes, ordering, paging and tracking.</summary>
public class EfSpecificationEvaluator : IEfSpecificationEvaluator
{
    private readonly EfSpecificationOptions _options;

    #region Construction

    /// <summary>
    /// Protected so instances can only be created through <see cref="Create"/>.
    /// </summary>
    protected EfSpecificationEvaluator(EfSpecificationOptions options)
    {
        _options = options;
    }

    #endregion

    #region Factory

    /// <summary>Creates an evaluator from the supplied options.</summary>
    public static Result<EfSpecificationEvaluator> Create(IOptions<EfSpecificationOptions> options)
        => EfSpecificationGuard.OptionsRequired(options)
            .Map(valid => new EfSpecificationEvaluator(valid));

    #endregion

    #region Evaluation

    /// <inheritdoc />
    public Result<IQueryable<T>> Apply<T>(
        IQueryable<T> query,
        ISpecification<T> specification)
        where T : class
        => EfSpecificationGuard.QueryRequired(query)
            .Bind(validQuery => EfSpecificationGuard
                .SpecificationRequired(specification)
                .Map(validSpec => ApplyCore(validQuery, validSpec)));

    #endregion

    #region Composition

    private IQueryable<T> ApplyCore<T>(
        IQueryable<T> query,
        ISpecification<T> specification)
        where T : class
    {
        if (!specification.IsTrackingEnabled)
        {
            query = query.AsNoTracking();
        }

        if (_options.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query.Where(specification.Criteria);

        foreach (Expression<Func<T, object>> include in specification.Includes)
        {
            query = query.Include(include);
        }

        foreach (string include in specification.IncludeStrings)
        {
            query = query.Include(include);
        }

        if (_options.UseSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        query = ApplyOrdering(query, specification);
        return ApplyPaging(query, specification);
    }

    private static IQueryable<T> ApplyOrdering<T>(
        IQueryable<T> query,
        ISpecification<T> specification)
        where T : class
    {
        if (specification.OrderBy.Count == 0)
        {
            return query;
        }

        IOrderedQueryable<T>? ordered = null;
        foreach ((Expression<Func<T, object>> keySelector, bool descending) in specification.OrderBy)
        {
            ordered = ordered is null
                ? descending
                    ? query.OrderByDescending(keySelector)
                    : query.OrderBy(keySelector)
                : descending
                    ? ordered.ThenByDescending(keySelector)
                    : ordered.ThenBy(keySelector);
        }

        return ordered!;
    }

    private static IQueryable<T> ApplyPaging<T>(
        IQueryable<T> query,
        ISpecification<T> specification)
        where T : class
    {
        if (!specification.IsPagingEnabled)
        {
            return query;
        }

        query = query.Skip(specification.Skip ?? 0);
        return specification.Take is { } take ? query.Take(take) : query;
    }

    #endregion
}
