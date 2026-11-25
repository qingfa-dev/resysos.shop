namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="QueryStringParser"/>.</summary>
public sealed class QueryStringParserTests
{
    #region Paging

    [Fact]
    public void Parse_NoInput_ShouldReturnEmptyRequest()
    {
        var result = QueryStringParser.Parse(filter: null, orderBy: null, page: 1, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Filters.Should().BeEmpty();
        result.Value.Sorts.Should().BeEmpty();
        result.Value.Paging.Page.Should().Be(1);
        result.Value.Paging.PageSize.Should().Be(20);
    }

    [Fact]
    public void Parse_ShouldClampPaging()
    {
        var result = QueryStringParser.Parse(null, null, page: 0, pageSize: 10_000);

        result.Value.Paging.Page.Should().Be(1);
        result.Value.Paging.PageSize.Should().Be(PagedRequest.MaxPageSize);
    }

    #endregion

    #region Filters

    [Theory]
    [InlineData("name==ada", FilterOperator.Eq, "ada")]
    [InlineData("name!=ada", FilterOperator.Ne, "ada")]
    [InlineData("total>=10", FilterOperator.Gte, "10")]
    [InlineData("total>10", FilterOperator.Gt, "10")]
    [InlineData("total<=10", FilterOperator.Lte, "10")]
    [InlineData("total<10", FilterOperator.Lt, "10")]
    [InlineData("name@=a", FilterOperator.Contains, "a")]
    [InlineData("name^=a", FilterOperator.StartsWith, "a")]
    [InlineData("name$=a", FilterOperator.EndsWith, "a")]
    public void Parse_FilterOperator_ShouldMap(string filter, FilterOperator expected, string value)
    {
        var result = QueryStringParser.Parse(filter, null, 1, 20);

        var descriptor = result.Value.Filters.Should().ContainSingle().Which;
        descriptor.Field.Should().Be(filter[..filter.IndexOfAny(['=', '!', '>', '<', '@', '^', '$'])]);
        descriptor.Operator.Should().Be(expected);
        descriptor.Value.Should().Be(value);
    }

    [Fact]
    public void Parse_InOperator_ShouldCollectValues()
    {
        var result = QueryStringParser.Parse("status|=new|open", null, 1, 20);

        var descriptor = result.Value.Filters.Should().ContainSingle().Which;
        descriptor.Operator.Should().Be(FilterOperator.In);
        descriptor.Values.Should().Equal("new", "open");
    }

    [Fact]
    public void Parse_MultipleFilters_ShouldSplitOnCommas()
    {
        var result = QueryStringParser.Parse("name==ada,total>=10", null, 1, 20);

        result.Value.Filters.Should().HaveCount(2);
        result.Value.Filters[0].Field.Should().Be("name");
        result.Value.Filters[1].Field.Should().Be("total");
    }

    [Fact]
    public void Parse_QuotedComma_ShouldStayInOneFilter()
    {
        var result = QueryStringParser.Parse("name=='a,b'", null, 1, 20);

        var descriptor = result.Value.Filters.Should().ContainSingle().Which;
        descriptor.Value.Should().Be("a,b");
    }

    [Fact]
    public void Parse_NullChecks_ShouldMap()
    {
        var result = QueryStringParser.Parse("name==null,age!=null", null, 1, 20);

        result.Value.Filters[0].Operator.Should().Be(FilterOperator.IsNull);
        result.Value.Filters[1].Operator.Should().Be(FilterOperator.IsNotNull);
    }

    [Fact]
    public void Parse_ExplicitNullWords_ShouldMap()
    {
        var result = QueryStringParser.Parse("name isnull,age isnotnull", null, 1, 20);

        result.Value.Filters[0].Operator.Should().Be(FilterOperator.IsNull);
        result.Value.Filters[1].Operator.Should().Be(FilterOperator.IsNotNull);
    }

    [Fact]
    public void Parse_QuotedNull_ShouldBeLiteralText()
    {
        var result = QueryStringParser.Parse("name=='null'", null, 1, 20);

        result.Value.Filters[0].Operator.Should().Be(FilterOperator.Eq);
        result.Value.Filters[0].Value.Should().Be("null");
    }

    [Fact]
    public void Parse_DottedField_ShouldBeAccepted()
    {
        var result = QueryStringParser.Parse("address.city==paris", null, 1, 20);

        result.Value.Filters[0].Field.Should().Be("address.city");
    }

    [Fact]
    public void Parse_FlatAnd_ShouldStillPopulateDescriptors()
    {
        var request = QueryStringParser.Parse("age>=18,name==Ada", null, 1, 20).Value;

        request.Filters.Should().HaveCount(2);
        request.Filter.Should().NotBeNull();
    }

    [Fact]
    public void Parse_NoFilter_ShouldLeaveFilterNull()
    {
        var request = QueryStringParser.Parse(null, null, 1, 20).Value;

        request.Filter.Should().BeNull();
        request.Filters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_OrFilter_ShouldPopulateFilterButNotDescriptors()
    {
        var request = QueryStringParser.Parse("name==ada or total>=10", null, 1, 20).Value;

        request.Filter.Should().NotBeNull();
        request.Filter.Should().BeOfType<FilterGroup>()
            .Which.Operator.Should().Be(FilterGroupOperator.Or);
        request.Filters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NotFilter_ShouldNotPopulateDescriptors()
    {
        var request = QueryStringParser.Parse("not name==ada", null, 1, 20).Value;

        request.Filter.Should().BeOfType<FilterNegation>();
        request.Filters.Should().BeEmpty();
    }

    #endregion

    #region Sorts

    [Fact]
    public void Parse_SortPrefixesAndSuffixes_ShouldMap()
    {
        var result = QueryStringParser.Parse(null, "name,-total,created asc,updated desc", 1, 20);

        result.Value.Sorts.Should().HaveCount(4);
        result.Value.Sorts[0].Direction.Should().Be(SortDirection.Ascending);
        result.Value.Sorts[1].Field.Should().Be("total");
        result.Value.Sorts[1].Direction.Should().Be(SortDirection.Descending);
        result.Value.Sorts[2].Direction.Should().Be(SortDirection.Ascending);
        result.Value.Sorts[3].Direction.Should().Be(SortDirection.Descending);
    }

    #endregion

    #region Errors

    [Fact]
    public void Parse_EmptyFilterClause_ShouldReturnError()
    {
        var result = QueryStringParser.Parse("name==ada,", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_FieldWithoutOperator_ShouldCarryField()
    {
        var result = QueryStringParser.Parse("name", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
        result.Errors[0].GetField().Should().Be("name");
    }

    [Fact]
    public void Parse_UnknownOperator_ShouldReturnError()
    {
        var result = QueryStringParser.Parse("name??ada", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_InvalidFieldName_ShouldReturnError()
    {
        var result = QueryStringParser.Parse("1name==ada", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_NullCheckWithValue_ShouldReturnError()
    {
        var result = QueryStringParser.Parse("name isnull extra", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_InvalidSortField_ShouldReturnError()
    {
        var result = QueryStringParser.Parse(null, "1name", 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_sort");
    }

    [Fact]
    public void Parse_EmptySortClause_ShouldReturnError()
    {
        var result = QueryStringParser.Parse(null, "name,", 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_sort");
    }

    [Fact]
    public void Parse_FilterInvalid_ShouldNotEvaluateSorts()
    {
        var result = QueryStringParser.Parse("name", "1bad", 1, 20);

        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_LexError_ShouldSurfaceAsInvalidFilter()
    {
        var result = QueryStringParser.Parse("name eq 'unterminated", null, 1, 20);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    #endregion
}
