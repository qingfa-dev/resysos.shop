using BuildingBlock.Contracts.Filtering;
using BuildingBlock.Contracts.Paging;
using BuildingBlock.Contracts.Sorting;

namespace BuildingBlock.Contracts.Querying;

/// <summary>
/// A complete list request: how to page it, which clauses filter it and how it is ordered.
/// It is transport-neutral; the query string is only one way to fill it.
/// </summary>
public sealed record QueryRequest
{
    #region State

    /// <summary>Gets the paging request; defaults to the first page.</summary>
    public PagedRequest Paging { get; init; } = new();

    /// <summary>Gets the filter clauses, combined with AND.</summary>
    public IReadOnlyList<FilterDescriptor> Filters { get; init; } = [];

    /// <summary>
    /// Gets the full filter expression tree, including OR, NOT and parentheses. It is populated
    /// for every parsed filter, while <see cref="Filters"/> is populated only when the expression
    /// is a flat AND of conditions.
    /// </summary>
    public FilterNode? Filter { get; init; }

    /// <summary>Gets the sort clauses, applied in order.</summary>
    public IReadOnlyList<SortDescriptor> Sorts { get; init; } = [];

    #endregion

    #region Factory

    /// <summary>Creates a request that only pages.</summary>
    /// <returns>A request with no filters and no sorts.</returns>
    public static QueryRequest PageOnly()
        => new();

    #endregion
}
