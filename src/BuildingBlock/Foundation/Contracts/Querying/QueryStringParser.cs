using BuildingBlock.Contracts.Filtering;
using BuildingBlock.Contracts.Paging;
using BuildingBlock.Contracts.Sorting;

namespace BuildingBlock.Contracts.Querying;

/// <summary>
/// Parses the <c>filter</c> and <c>orderBy</c> query-string values into a
/// <see cref="QueryRequest"/>. Client input never throws: malformed text becomes a named
/// validation error on the railway.
/// </summary>
/// <remarks>
/// <para>
/// A filter clause is <c>field operator value</c>. Supported operators are
/// <c>==</c>, <c>!=</c>, <c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>, <c>&lt;=</c>,
/// <c>@=</c> (contains), <c>^=</c> (starts with), <c>$=</c> (ends with),
/// <c>|=</c> (any of, values separated by <c>|</c>), <c>isnull</c> and <c>isnotnull</c>.
/// </para>
/// <para>
/// Clauses are separated by commas. Wrap a value in single quotes to keep a comma in it.
/// An unquoted <c>null</c> becomes a null check; quote it for the literal text.
/// </para>
/// <para>
/// A sort clause is a field name, optionally prefixed with <c>-</c> or suffixed with
/// <c>asc</c> or <c>desc</c>. Sorts are separated by commas.
/// </para>
/// <para>
/// Example: <c>?filter=name@=a,total&gt;=10&amp;orderBy=name desc&amp;page=2&amp;pageSize=10</c>
/// </para>
/// </remarks>
public static partial class QueryStringParser
{
    #region Parse

    /// <summary>Parses the raw query-string values into a request.</summary>
    /// <param name="filter">The raw <c>filter</c> value, or null when absent.</param>
    /// <param name="orderBy">The raw <c>orderBy</c> value, or null when absent.</param>
    /// <param name="page">The requested page; clamped by <see cref="PagedRequest"/>.</param>
    /// <param name="pageSize">The requested page size; clamped by <see cref="PagedRequest"/>.</param>
    /// <returns>The request, or a validation error describing the first bad clause.</returns>
    public static Result<QueryRequest> Parse(
        string? filter,
        string? orderBy,
        int page,
        int pageSize)
    {
        var filters = ParseFilters(filter);
        if (filters.IsFailure)
        {
            return Result.Failure<QueryRequest>(filters.Errors);
        }

        var sorts = ParseSorts(orderBy);
        if (sorts.IsFailure)
        {
            return Result.Failure<QueryRequest>(sorts.Errors);
        }

        return QueryGuard.Complete(new QueryRequest
        {
            Paging = new PagedRequest { Page = page, PageSize = pageSize },
            Filters = filters.Value,
            Sorts = sorts.Value
        });
    }

    #endregion

    #region Lists

    private static Result<IReadOnlyList<FilterDescriptor>> ParseFilters(string? filter)
    {
        List<FilterDescriptor> descriptors = [];
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Result.Success<IReadOnlyList<FilterDescriptor>>(descriptors);
        }

        foreach (string clause in SplitClauses(filter))
        {
            var parsed = ParseClause(clause);
            if (parsed.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FilterDescriptor>>(parsed.Errors);
            }

            descriptors.Add(parsed.Value);
        }

        return Result.Success<IReadOnlyList<FilterDescriptor>>(descriptors);
    }

    private static Result<IReadOnlyList<SortDescriptor>> ParseSorts(string? orderBy)
    {
        List<SortDescriptor> sorts = [];
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return Result.Success<IReadOnlyList<SortDescriptor>>(sorts);
        }

        foreach (string clause in SplitClauses(orderBy))
        {
            var parsed = ParseSortClause(clause);
            if (parsed.IsFailure)
            {
                return Result.Failure<IReadOnlyList<SortDescriptor>>(parsed.Errors);
            }

            sorts.Add(parsed.Value);
        }

        return Result.Success<IReadOnlyList<SortDescriptor>>(sorts);
    }

    #endregion
}
