namespace BuildingBlock.Contracts.Unit.Tests.Paging;

/// <summary>Tests for <see cref="PagingConstant"/>.</summary>
public sealed class PagingConstantTests
{
    #region Constraints

    [Fact]
    public void Constraints_ShouldMatchDocumentedValues()
    {
        PagingConstant.Constraints.Page.Min.Value.Should().Be(1);
        PagingConstant.Constraints.PageSize.Min.Value.Should().Be(1);
        PagingConstant.Constraints.PageSize.Max.Value.Should().Be(200);
    }

    [Fact]
    public void MinAndMaxErrors_ShouldShareCodeAndCarryMessages()
    {
        var min = PagingConstant.Constraints.PageSize.Min.Error;
        var max = PagingConstant.Constraints.PageSize.Max.Error;

        min.Code.Should().Be("paging.page_size.invalid");
        max.Code.Should().Be("paging.page_size.invalid");
        min.Type.Should().Be(ErrorType.Validation);
        min.Message.Should().Contain("at least 1");
        max.Message.Should().Contain("cannot exceed 200");
    }

    [Fact]
    public void PageError_ShouldHaveDocumentedIdentity()
    {
        var error = PagingConstant.Constraints.Page.Min.Error;

        error.Code.Should().Be("paging.page.invalid");
        error.Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        PagingConstant.Defaults.Page.Should().Be(1);
        PagingConstant.Defaults.PageSize.Should().Be(20);
    }

    #endregion
}
