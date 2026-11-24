namespace BuildingBlock.Contracts.Paging;

/// <summary>
/// Standard paged result returned by queries and public API calls.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PagedResult<T>
{
    #region State

    /// <summary>Gets the items on the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Gets the 1-based page number.</summary>
    public int Page { get; init; } = PagingConstant.Defaults.Page;

    /// <summary>Gets the page size.</summary>
    public int PageSize { get; init; } = PagingConstant.Defaults.PageSize;

    /// <summary>Gets the total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    #endregion

    #region Derived

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize == 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Gets whether another page exists after this one.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets whether a page exists before this one.</summary>
    public bool HasPreviousPage => Page > 1;

    #endregion

    #region Factory

    /// <summary>Creates an empty page.</summary>
    /// <param name="request">The paging request.</param>
    /// <returns>An empty paged result.</returns>
    public static PagedResult<T> Empty(PagedRequest? request = null)
        => new()
        {
            Page = request?.Page ?? PagingConstant.Defaults.Page,
            PageSize = request?.PageSize ?? PagingConstant.Defaults.PageSize
        };

    /// <summary>Creates a page from a set of items and a total count.</summary>
    /// <param name="items">The items on the page.</param>
    /// <param name="request">The paging request.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>The paged result.</returns>
    public static PagedResult<T> Create(
        IEnumerable<T> items,
        PagedRequest request,
        int totalCount)
        => new()
        {
            Items = items.ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

    #endregion
}
