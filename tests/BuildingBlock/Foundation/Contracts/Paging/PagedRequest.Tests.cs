namespace BuildingBlock.Contracts.Unit.Tests.Paging;

/// <summary>Tests for <see cref="PagedRequest"/>.</summary>
public sealed class PagedRequestTests
{
    #region Defaults

    [Fact]
    public void Defaults_ShouldUseFirstPageAndDefaultSize()
    {
        var request = new PagedRequest();

        request.Page.Should().Be(PagingConstant.Defaults.Page);
        request.PageSize.Should().Be(PagingConstant.Defaults.PageSize);
        request.Skip.Should().Be(0);
    }

    [Fact]
    public void Skip_ShouldBeZeroBased()
    {
        var request = new PagedRequest { Page = 3, PageSize = 10 };

        request.Skip.Should().Be(20);
    }

    #endregion

    #region Clamping

    [Fact]
    public void Page_BelowOne_ShouldClampToOne()
    {
        new PagedRequest { Page = 0 }.Page.Should().Be(1);
    }

    [Fact]
    public void PageSize_AboveMax_ShouldClampToMax()
    {
        new PagedRequest { PageSize = 10_000 }.PageSize.Should().Be(PagedRequest.MaxPageSize);
    }

    [Fact]
    public void PageSize_BelowOne_ShouldClampToMax()
    {
        new PagedRequest { PageSize = 0 }.PageSize.Should().Be(PagedRequest.MaxPageSize);
    }

    #endregion

    #region Factory

    [Fact]
    public void Create_ValidInput_ShouldSucceed()
    {
        var result = PagedRequest.Create(page: 2, pageSize: 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public void Create_Defaults_ShouldUseConstantDefaults()
    {
        var result = PagedRequest.Create();

        result.Value.Page.Should().Be(PagingConstant.Defaults.Page);
        result.Value.PageSize.Should().Be(PagingConstant.Defaults.PageSize);
    }

    [Fact]
    public void Create_InvalidPage_ShouldReturnError()
    {
        PagedRequest.Create(page: 0, pageSize: 10).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.Page.Min.Error);
    }

    [Fact]
    public void Create_InvalidPageSize_ShouldReturnError()
    {
        PagedRequest.Create(page: 1, pageSize: 1000).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.PageSize.Max.Error);
    }

    #endregion
}
