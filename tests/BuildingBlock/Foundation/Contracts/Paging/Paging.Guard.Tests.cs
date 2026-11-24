namespace BuildingBlock.Contracts.Unit.Tests.Paging;

/// <summary>Tests for <see cref="PagingGuard"/>.</summary>
public sealed class PagingGuardTests
{
    #region Page

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void ValidPage_InRange_ShouldSucceed(int page)
    {
        PagingGuard.ValidPage(page).Value.Should().Be(page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidPage_OutOfRange_ShouldReturnError(int page)
    {
        PagingGuard.ValidPage(page).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.Page.Min.Error);
    }

    #endregion

    #region Page Size

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(200)]
    public void ValidPageSize_InRange_ShouldSucceed(int pageSize)
    {
        PagingGuard.ValidPageSize(pageSize).Value.Should().Be(pageSize);
    }

    [Fact]
    public void ValidPageSize_BelowMinimum_ShouldReturnMinError()
    {
        PagingGuard.ValidPageSize(0).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.PageSize.Min.Error);
    }

    [Fact]
    public void ValidPageSize_AboveMaximum_ShouldReturnMaxError()
    {
        PagingGuard.ValidPageSize(201).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.PageSize.Max.Error);
    }

    #endregion

    #region Paging

    [Fact]
    public void ValidPaging_ValidInput_ShouldReturnBoth()
    {
        var result = PagingGuard.ValidPaging(page: 3, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public void ValidPaging_InvalidPage_ShouldReturnPageError()
    {
        PagingGuard.ValidPaging(page: 0, pageSize: 20).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.Page.Min.Error);
    }

    [Fact]
    public void ValidPaging_InvalidPageSize_ShouldReturnSizeError()
    {
        PagingGuard.ValidPaging(page: 1, pageSize: 0).Errors.Should().ContainSingle()
            .Which.Should().Be(PagingConstant.Constraints.PageSize.Min.Error);
    }

    #endregion
}
