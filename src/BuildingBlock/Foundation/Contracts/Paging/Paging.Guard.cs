namespace BuildingBlock.Contracts.Paging;

/// <summary>Guards for paging input. Failures stay on the railway.</summary>
public static class PagingGuard
{
    #region Page

    /// <summary>Checks a page number against the documented constraint.</summary>
    public static Result<int> ValidPage(int page)
        => page >= PagingConstant.Constraints.Page.Min.Value
            ? Result.Success(page)
            : Result.Failure<int>(PagingConstant.Constraints.Page.Min.Error);

    #endregion

    #region Page Size

    /// <summary>Checks a page size against the documented constraints.</summary>
    public static Result<int> ValidPageSize(int pageSize)
        => pageSize < PagingConstant.Constraints.PageSize.Min.Value
            ? Result.Failure<int>(PagingConstant.Constraints.PageSize.Min.Error)
            : pageSize > PagingConstant.Constraints.PageSize.Max.Value
                ? Result.Failure<int>(PagingConstant.Constraints.PageSize.Max.Error)
                : Result.Success(pageSize);

    #endregion

    #region Paging

    /// <summary>Validates a page and page size together.</summary>
    public static Result<(int Page, int PageSize)> ValidPaging(int page, int pageSize)
    {
        var validPage = ValidPage(page);
        if (validPage.IsFailure)
            return Result.Failure<(int Page, int PageSize)>(validPage.Errors);

        var validSize = ValidPageSize(pageSize);
        if (validSize.IsFailure)
            return Result.Failure<(int Page, int PageSize)>(validSize.Errors);

        return Result.Success((validPage.Value, validSize.Value));
    }

    #endregion
}
