namespace BuildingBlock.Querying.Dapper.Unit.Tests;

/// <summary>Tests for <see cref="DapperQueryPlanExecutor"/> over materialized sequences.</summary>
public sealed class DapperQueryPlanExecutorTests
{
    #region Build

    [Fact]
    public void Build_AppliesFilterSortAndPaging()
    {
        var plan = new QueryPlan<Person>
        {
            Filter = person => person.Age >= 18,
            Sorts = [new QuerySort<Person>(person => person.Age, Descending: true)],
            Paging = new PagingPlan.Offset(0, 2)
        };

        var result = DapperQueryPlanExecutor.Create().Value.Build(People.All, plan).Value;

        People.Names(result).Should().Equal("Grace", "Ada");
    }

    [Fact]
    public void Build_OverSequenceWithNullSource_ShouldReturnError()
    {
        var executor = DapperQueryPlanExecutor.Create().Value;

        var result = executor.Build<Person>((IEnumerable<Person>)null!, new QueryPlan<Person>());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.dapper.source.required");
    }

    [Fact]
    public void Build_WithNullPlan_ShouldReturnError()
    {
        var executor = DapperQueryPlanExecutor.Create().Value;

        var result = executor.Build(People.All, null!);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.dapper.plan.required");
    }

    #endregion

    #region Execute

    [Fact]
    public void Execute_CountsFilteredSetBeforePaging()
    {
        var plan = new QueryPlan<Person>
        {
            Filter = person => person.Age >= 18,
            Sorts = [new QuerySort<Person>(person => person.Age, Descending: false)],
            Paging = new PagingPlan.Offset(0, 2),
            IncludeTotalCount = true
        };

        var page = DapperQueryPlanExecutor.Create().Value.Execute(People.All, plan).Value;

        page.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(3);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSamePageAsSync()
    {
        var plan = new QueryPlan<Person> { Paging = new PagingPlan.Offset(0, 3) };
        var executor = DapperQueryPlanExecutor.Create().Value;

        var asyncPage = (await executor.ExecuteAsync(People.All.AsQueryable(), plan)).Value;
        var syncPage = executor.Execute(People.All.AsQueryable(), plan).Value;

        asyncPage.Items.Should().BeEquivalentTo(syncPage.Items);
        asyncPage.TotalCount.Should().Be(syncPage.TotalCount);
    }

    #endregion
}
