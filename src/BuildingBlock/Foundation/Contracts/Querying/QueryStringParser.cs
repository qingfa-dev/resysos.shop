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
/// The <c>filter</c> value is a boolean expression parsed by <see cref="FilterParser"/>:
/// conditions combine with <c>and</c> (or a comma), <c>or</c>, <c>not</c> and parentheses.
/// A condition is <c>field operator value</c> with word operators (<c>eq</c>, <c>neq</c>,
/// <c>gt</c>, <c>gte</c>, <c>lt</c>, <c>lte</c>, <c>contains</c>, <c>startswith</c>,
/// <c>endswith</c>, <c>in</c>, <c>notin</c>, <c>between</c>, <c>isnull</c>, <c>isnotnull</c>)
/// or their legacy symbol aliases (<c>==</c>, <c>!=</c>, <c>&gt;</c>, <c>&gt;=</c>,
/// <c>&lt;</c>, <c>&lt;=</c>, <c>@=</c>, <c>^=</c>, <c>$=</c>, <c>|=</c>).
/// </para>
/// <para>
/// The full tree is exposed on <see cref="QueryRequest.Filter"/>; the flat-AND projection
/// <see cref="QueryRequest.Filters"/> is populated only when the expression is an AND of
/// conditions. Wrap a value in single quotes to keep a comma in it. An unquoted <c>null</c>
/// becomes a null check; quote it for the literal text.
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
        var filters = ParseFilter(filter);
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
            Filter = filters.Value.Filter,
            Filters = filters.Value.Filters,
            Sorts = sorts.Value
        });
    }

    #endregion

    #region Lists

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
