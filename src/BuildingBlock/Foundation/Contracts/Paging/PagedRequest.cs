namespace BuildingBlock.Contracts.Paging;

/// <summary>
/// Standard paging input for queries and public API calls.
/// </summary>
public sealed record PagedRequest
{
    #region Constants

    /// <summary>The maximum page size a caller may request.</summary>
    public const int MaxPageSize = PagingConstant.Constraints.PageSize.Max.Value;

    #endregion

    #region State

    private readonly int _page = PagingConstant.Defaults.Page;
    private readonly int _pageSize = PagingConstant.Defaults.PageSize;

    /// <summary>Gets the 1-based page number.</summary>
    public int Page
    {
        get => _page;
        init => _page = value < PagingConstant.Constraints.Page.Min.Value
            ? PagingConstant.Constraints.Page.Min.Value
            : value;
    }

    /// <summary>Gets the requested page size, capped at <see cref="MaxPageSize"/>.</summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value is < PagingConstant.Constraints.PageSize.Min.Value or > MaxPageSize
            ? MaxPageSize
            : value;
    }

    /// <summary>Gets the number of items to skip.</summary>
    public int Skip => (Page - 1) * PageSize;

    #endregion

    #region Factory

    /// <summary>
    /// Creates a paging request, returning validation errors instead of silently clamping.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>The request, or a validation error.</returns>
    public static Result<PagedRequest> Create(
        int page = PagingConstant.Defaults.Page,
        int pageSize = PagingConstant.Defaults.PageSize)
        => PagingGuard.ValidPaging(page, pageSize)
            .Map(valid => new PagedRequest { Page = valid.Page, PageSize = valid.PageSize });

    #endregion
}
