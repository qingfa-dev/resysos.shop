namespace BuildingBlock.Contracts.Unit.Tests.Paging;

/// <summary>Tests for <see cref="PagedResult{T}"/>.</summary>
public sealed class PagedResultTests
{
    #region Empty

    [Fact]
    public void Empty_ShouldUseDefaults()
    {
        var result = PagedResult<string>.Empty();

        result.Items.Should().BeEmpty();
        result.Page.Should().Be(PagingConstant.Defaults.Page);
        result.PageSize.Should().Be(PagingConstant.Defaults.PageSize);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Empty_WithRequest_ShouldCarryPaging()
    {
        var result = PagedResult<string>.Empty(new PagedRequest { Page = 3, PageSize = 5 });

        result.Page.Should().Be(3);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public void Empty_WhenPageSizeZero_ShouldReportNoPages()
    {
        var result = PagedResult<string>.Empty(new PagedRequest { PageSize = 1 }) with
        {
            PageSize = 0,
            TotalCount = 10
        };

        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region Pagination

    [Fact]
    public void TotalPages_ShouldRoundUp()
    {
        var result = new PagedResult<int> { TotalCount = 21, PageSize = 10 };

        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasNextPage_ShouldReflectPosition()
    {
        var first = new PagedResult<int> { Page = 1, PageSize = 10, TotalCount = 30 };
        var last = new PagedResult<int> { Page = 3, PageSize = 10, TotalCount = 30 };

        first.HasNextPage.Should().BeTrue();
        last.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ShouldReflectPosition()
    {
        var first = new PagedResult<int> { Page = 1, PageSize = 10, TotalCount = 30 };
        var second = new PagedResult<int> { Page = 2, PageSize = 10, TotalCount = 30 };

        first.HasPreviousPage.Should().BeFalse();
        second.HasPreviousPage.Should().BeTrue();
    }

    #endregion

    #region Factory

    [Fact]
    public void Create_ShouldCarryItemsAndPaging()
    {
        var request = new PagedRequest { Page = 2, PageSize = 2 };

        var result = PagedResult<int>.Create([3, 4], request, totalCount: 5);

        result.Items.Should().Equal(3, 4);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    #endregion
}
