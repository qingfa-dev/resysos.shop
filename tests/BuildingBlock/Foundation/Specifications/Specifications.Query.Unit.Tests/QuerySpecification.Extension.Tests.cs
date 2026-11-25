namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for <see cref="QuerySpecificationExtensions"/>.</summary>
public sealed class QuerySpecificationExtensionTests
{
    #region Mapping

    [Fact]
    public void ToSpecification_ShouldMapAllConcerns()
    {
        var request = Mapping.Request(
            filters: [Mapping.Filter("age", FilterOperator.Gte, "18")],
            sorts: [new SortDescriptor { Field = "age", Direction = SortDirection.Descending }],
            paging: new PagedRequest { Page = 2, PageSize = 5 });

        var result = request.ToSpecification<Person>();

        result.IsSuccess.Should().BeTrue();
        var specification = result.Value;
        specification.IsPagingEnabled.Should().BeTrue();
        specification.Skip.Should().Be(5);
        specification.Take.Should().Be(5);
        specification.OrderBy.Should().ContainSingle();
        specification.OrderBy[0].Descending.Should().BeTrue();
        specification.IsSatisfiedBy(People.Ada).Should().BeTrue();
        specification.IsSatisfiedBy(People.Alan).Should().BeFalse();
    }

    [Fact]
    public void ToSpecification_CombinedConcerns_ShouldApplyFilterSortAndPaging()
    {
        var request = Mapping.Request(
            filters: [Mapping.Filter("salary", FilterOperator.Gte, "1000")],
            sorts: [new SortDescriptor { Field = "salary" }],
            paging: new PagedRequest { Page = 2, PageSize = 1 });

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Ada");
    }

    [Fact]
    public void ToSpecification_WithNullRequest_ShouldReturnRequiredError()
    {
        QueryRequest? request = null;

        var result = request.ToSpecification<Person>();

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Request.Required.Error);
    }

    #endregion

    #region Query Parameters

    [Fact]
    public void ToSpecification_FromQueryParameters_ShouldParseAndMap()
    {
        var parameters = new QueryParameters
        {
            Filter = "age>=18,status==Active",
            OrderBy = "age desc",
            Page = 1,
            PageSize = 10
        };

        var result = parameters.ToSpecification<Person>()
            .Bind(specification => InMemorySpecificationEvaluator.Create()
                .Bind(evaluator => evaluator.Apply(People.All, specification)));

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value).Should().Equal("Ada", "Noah");
    }

    [Fact]
    public void ToSpecification_FromNullQueryParameters_ShouldReturnRequiredError()
    {
        QueryParameters? parameters = null;

        var result = parameters.ToSpecification<Person>();

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Parameters.Required.Error);
    }

    [Fact]
    public void ToSpecification_FromInvalidQueryParameters_ShouldReturnParserError()
    {
        var parameters = new QueryParameters { Filter = "1name==a" };

        var result = parameters.ToSpecification<Person>();

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.invalid_filter");
    }

    #endregion

    #region Mapper Selection

    [Fact]
    public void ToSpecification_WithCustomMapper_ShouldDelegateToIt()
    {
        var mapper = new StubMapper();

        var result = Mapping.Request().ToSpecification<Person>(mapper);

        mapper.Called.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("stub.mapper");
    }

    [Fact]
    public void ToSpecification_WithoutMapper_ShouldUseSharedInstance()
    {
        var result = Mapping.Request().ToSpecification<Person>();

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Safety

    [Fact]
    public void ToSpecification_InvalidInput_ShouldNotThrow()
    {
        var request = Mapping.Request(filters:
        [
            Mapping.Filter("nickname", FilterOperator.Eq, "A"),
            Mapping.Filter("age", FilterOperator.Eq, "abc")
        ]);

        var act = () => request.ToSpecification<Person>();

        act.Should().NotThrow().Which.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Stub

    private sealed class StubMapper : IQuerySpecificationMapper
    {
        public bool Called { get; private set; }

        public Result<ISpecification<T>> Map<T>(QueryRequest? request)
            where T : class
        {
            Called = true;
            return Result.Failure<ISpecification<T>>(
                Error.Validation("stub.mapper", "The stub mapper was called."));
        }
    }

    #endregion
}
