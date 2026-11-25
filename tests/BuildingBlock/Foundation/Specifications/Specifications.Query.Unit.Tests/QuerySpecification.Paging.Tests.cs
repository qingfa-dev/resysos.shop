namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for paging mapping.</summary>
public sealed class QuerySpecificationPagingTests
{
    #region Pages

    [Fact]
    public void FirstPage_ShouldTakeRequestedSize()
    {
        var request = Mapping.Request(paging: new PagedRequest { Page = 1, PageSize = 2 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Ada", "Grace");
    }

    [Fact]
    public void SecondPage_ShouldSkipFirstPage()
    {
        var request = Mapping.Request(paging: new PagedRequest { Page = 2, PageSize = 2 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Alan", "Noah");
    }

    [Fact]
    public void PageBeyondData_ShouldBeEmpty()
    {
        var request = Mapping.Request(paging: new PagedRequest { Page = 3, PageSize = 2 });

        var result = Mapping.Evaluate(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Paging_ShouldApplyAfterSorting()
    {
        var request = Mapping.Request(
            sorts: [new SortDescriptor { Field = "age" }],
            paging: new PagedRequest { Page = 1, PageSize = 2 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Alan", "Noah");
    }

    #endregion

    #region Clamping

    [Fact]
    public void PageBelowMinimum_ShouldClampToFirstPage()
    {
        var request = Mapping.Request(paging: new PagedRequest { Page = 0, PageSize = 2 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Ada", "Grace");
    }

    [Fact]
    public void PageSizeAboveMaximum_ShouldClampToMaximum()
    {
        var request = Mapping.Request(paging: new PagedRequest { PageSize = 1000 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Ada", "Grace", "Alan", "Noah");
    }

    #endregion

    #region Specification State

    [Fact]
    public void MappedSpecification_ShouldExposeSkipAndTake()
    {
        var request = Mapping.Request(paging: new PagedRequest { Page = 2, PageSize = 10 });

        var specification = Mapping.Specification(request).Value;

        specification.IsPagingEnabled.Should().BeTrue();
        specification.Skip.Should().Be(10);
        specification.Take.Should().Be(10);
    }

    [Fact]
    public void DefaultRequest_ShouldUseDefaultPaging()
    {
        var specification = Mapping.Specification(Mapping.Request()).Value;

        specification.IsPagingEnabled.Should().BeTrue();
        specification.Skip.Should().Be(0);
        specification.Take.Should().Be(PagingConstant.Defaults.PageSize);
    }

    #endregion

    #region Invalid Input

    [Fact]
    public void NullPaging_ShouldReturnRequiredError()
    {
        var request = new QueryRequest { Paging = null! };

        var result = Mapping.Specification(request);

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Paging.Required.Error);
    }

    #endregion
}
