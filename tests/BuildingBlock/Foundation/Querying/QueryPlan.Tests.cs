namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryPlan{T}"/> and its plan components.</summary>
public sealed class QueryPlanTests
{
    #region Defaults

    [Fact]
    public void NewPlan_ShouldHaveNoFilterAndEmptySorts()
    {
        var plan = new QueryPlan<Person>();

        plan.Filter.Should().BeNull();
        plan.Sorts.Should().BeEmpty();
        plan.Paging.Should().Be(PagingPlan.None);
        plan.IncludeTotalCount.Should().BeFalse();
    }

    #endregion

    #region Paging

    [Fact]
    public void OffsetPaging_ShouldExposeSkipAndTake()
    {
        var paging = new PagingPlan.Offset(20, 10);

        paging.Skip.Should().Be(20);
        paging.Take.Should().Be(10);
    }

    #endregion
}
