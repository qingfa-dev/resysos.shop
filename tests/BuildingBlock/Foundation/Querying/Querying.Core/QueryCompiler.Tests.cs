namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryCompiler"/>.</summary>
public sealed class QueryCompilerTests
{
    #region Compile

    [Fact]
    public void Compile_MapsFilterSortAndOffsetPaging()
    {
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "age", Operator = FilterOperator.Gte, Value = "18" },
            Sorts = [new SortDescriptor { Field = "age", Direction = SortDirection.Descending }],
            Paging = new PagedRequest { Page = 2, PageSize = 10 }
        };

        var plan = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous).Value;

        plan.Filter.Should().NotBeNull();
        plan.Sorts.Should().ContainSingle().Which.Descending.Should().BeTrue();

        var paging = plan.Paging.Should().BeOfType<PagingPlan.Offset>().Which;
        paging.Skip.Should().Be(10);
        paging.Take.Should().Be(10);
    }

    #endregion

    #region Security

    [Fact]
    public void Compile_SecurityFailure_ShouldShortCircuit()
    {
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "passwordHash", Operator = FilterOperator.Eq, Value = "x" }
        };

        var result = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Paging

    [Fact]
    public void Compile_OffsetOverflow_ShouldClamp()
    {
        var request = new QueryRequest { Paging = new PagedRequest { Page = int.MaxValue, PageSize = int.MaxValue } };

        var plan = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous).Value;

        var paging = plan.Paging.Should().BeOfType<PagingPlan.Offset>().Which;
        paging.Take.Should().Be(Policy.For<Person>().MaxPageSize);
        paging.Skip.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Filters

    [Fact]
    public void Compile_FiltersOnly_ShouldCompileTheFilter()
    {
        var request = new QueryRequest
        {
            Filters =
            [
                new FilterDescriptor { Field = "age", Operator = FilterOperator.Gte, Value = "18" }
            ]
        };

        var plan = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous).Value;

        plan.Filter.Should().NotBeNull();
        var predicate = plan.Filter!.Compile();
        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 17 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_MultipleFiltersOnly_ShouldCombineWithAnd()
    {
        var request = new QueryRequest
        {
            Filters =
            [
                new FilterDescriptor { Field = "age", Operator = FilterOperator.Gte, Value = "18" },
                new FilterDescriptor { Field = "age", Operator = FilterOperator.Lte, Value = "40" }
            ]
        };

        var plan = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous).Value;

        var predicate = plan.Filter!.Compile();
        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 45 }).Should().BeFalse();
        predicate(new Person { Age = 17 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_UnknownFieldInFilters_ShouldShortCircuit()
    {
        var request = new QueryRequest
        {
            Filters = [new FilterDescriptor { Field = "passwordHash", Operator = FilterOperator.Eq, Value = "x" }]
        };

        var result = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Sorts

    [Fact]
    public void Compile_NullSortElement_ShouldNotThrow()
    {
        var request = new QueryRequest { Sorts = [null!] };

        var result = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sorts.Should().BeEmpty();
    }

    #endregion

    #region Defaults

    [Fact]
    public void Compile_NoFilterOrSorts_ShouldProduceMatchAllPlan()
    {
        var request = new QueryRequest();

        var plan = QueryCompiler.Compile<Person>(request, Policy.For<Person>(), QueryContext.Anonymous).Value;

        plan.Filter.Should().NotBeNull();
        plan.Filter!.Compile()(new Person()).Should().BeTrue();
        plan.Sorts.Should().BeEmpty();
        plan.Paging.Should().BeOfType<PagingPlan.Offset>();
        plan.IncludeTotalCount.Should().BeTrue();
    }

    [Fact]
    public void Compile_NullRequest_ShouldUseDefaults()
    {
        var plan = QueryCompiler.Compile<Person>(null, Policy.For<Person>(), QueryContext.Anonymous).Value;

        plan.Filter.Should().NotBeNull();
        plan.Sorts.Should().BeEmpty();
    }

    #endregion
}
