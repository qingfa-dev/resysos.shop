namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="QueryParameters"/>.</summary>
public sealed class QueryParametersTests
{
    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchQueryDefaults()
    {
        var parameters = new QueryParameters();

        parameters.Page.Should().Be(QueryConstant.Defaults.Page);
        parameters.PageSize.Should().Be(QueryConstant.Defaults.PageSize);
        parameters.Filter.Should().BeNull();
        parameters.OrderBy.Should().BeNull();
    }

    #endregion

    #region Parsing

    [Fact]
    public void ToRequest_ShouldParseBoundValues()
    {
        var parameters = new QueryParameters
        {
            Page = 2,
            PageSize = 10,
            Filter = "name@=a",
            OrderBy = "name desc"
        };

        var result = parameters.ToRequest();

        result.IsSuccess.Should().BeTrue();
        result.Value.Paging.Page.Should().Be(2);
        result.Value.Paging.PageSize.Should().Be(10);
        result.Value.Filters.Should().ContainSingle();
        result.Value.Sorts.Should().ContainSingle();
        result.Value.Sorts[0].Direction.Should().Be(SortDirection.Descending);
    }

    [Fact]
    public void ToRequest_InvalidInput_ShouldReturnError()
    {
        var parameters = new QueryParameters { Filter = "name" };

        var result = parameters.ToRequest();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    #endregion
}
