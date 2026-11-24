namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="QueryConstant"/>.</summary>
public sealed class QueryConstantTests
{
    #region Errors

    [Fact]
    public void RequestRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QueryConstant.Constraints.Request.Required.Error;

        error.Code.Should().Be("query.request.required");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be("A query request is required.");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchPagingDefaults()
    {
        QueryConstant.Defaults.Page.Should().Be(PagingConstant.Defaults.Page);
        QueryConstant.Defaults.PageSize.Should().Be(PagingConstant.Defaults.PageSize);
    }

    #endregion
}
