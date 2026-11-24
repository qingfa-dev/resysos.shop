namespace BuildingBlock.Contracts.Querying;

/// <summary>
/// The query-string shape bound by an endpoint, such as
/// <c>?filter=name@=a&amp;orderBy=name desc&amp;page=2&amp;pageSize=10</c>.
/// </summary>
public sealed class QueryParameters
{
    #region State

    /// <summary>Gets the 1-based page number.</summary>
    public int Page { get; init; } = QueryConstant.Defaults.Page;

    /// <summary>Gets the requested page size.</summary>
    public int PageSize { get; init; } = QueryConstant.Defaults.PageSize;

    /// <summary>Gets the raw filter expression.</summary>
    public string? Filter { get; init; }

    /// <summary>Gets the raw sort expression.</summary>
    public string? OrderBy { get; init; }

    #endregion

    #region Parsing

    /// <summary>Parses the bound values into a request.</summary>
    /// <returns>The request, or a validation error describing the first bad clause.</returns>
    public Result<QueryRequest> ToRequest()
        => QueryStringParser.Parse(Filter, OrderBy, Page, PageSize);

    #endregion
}
