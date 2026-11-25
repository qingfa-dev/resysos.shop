namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="FilterOperators"/>.</summary>
public sealed class FilterOperatorsTests
{
    #region Composition

    [Fact]
    public void FilterOperators_Flags_ShouldCompose()
    {
        var mask = FilterOperators.Gt | FilterOperators.Lt;

        mask.HasFlag(FilterOperators.Gt).Should().BeTrue();
        mask.HasFlag(FilterOperators.Lt).Should().BeTrue();
        mask.HasFlag(FilterOperators.Eq).Should().BeFalse();
    }

    [Fact]
    public void None_ShouldAllowNothing()
    {
        FilterOperators.None.Should().Be((FilterOperators)0);
        FilterOperators.None.HasFlag(FilterOperators.Eq).Should().BeFalse();
    }

    #endregion

    #region Bits

    private static readonly FilterOperators[] OperatorBits =
    [
        FilterOperators.Eq,
        FilterOperators.Ne,
        FilterOperators.Gt,
        FilterOperators.Gte,
        FilterOperators.Lt,
        FilterOperators.Lte,
        FilterOperators.Contains,
        FilterOperators.StartsWith,
        FilterOperators.EndsWith,
        FilterOperators.In,
        FilterOperators.IsNull,
        FilterOperators.IsNotNull,
        FilterOperators.NotIn,
        FilterOperators.Between,
        FilterOperators.Any,
        FilterOperators.CollectionAll
    ];

    [Fact]
    public void OperatorBits_ShouldBeDistinctPowersOfTwo()
    {
        OperatorBits.Should().OnlyHaveUniqueItems();
        OperatorBits.Should().OnlyContain(op => (int)op != 0 && ((int)op & ((int)op - 1)) == 0);
    }

    [Fact]
    public void All_ShouldIncludeEveryOperatorBit()
    {
        foreach (FilterOperators op in OperatorBits)
        {
            FilterOperators.All.HasFlag(op).Should().BeTrue();
        }
    }

    #endregion

    #region Mapping

    [Theory]
    [InlineData(FilterOperators.Eq, FilterOperator.Eq)]
    [InlineData(FilterOperators.Ne, FilterOperator.Ne)]
    [InlineData(FilterOperators.Gt, FilterOperator.Gt)]
    [InlineData(FilterOperators.Gte, FilterOperator.Gte)]
    [InlineData(FilterOperators.Lt, FilterOperator.Lt)]
    [InlineData(FilterOperators.Lte, FilterOperator.Lte)]
    [InlineData(FilterOperators.Contains, FilterOperator.Contains)]
    [InlineData(FilterOperators.StartsWith, FilterOperator.StartsWith)]
    [InlineData(FilterOperators.EndsWith, FilterOperator.EndsWith)]
    [InlineData(FilterOperators.In, FilterOperator.In)]
    [InlineData(FilterOperators.IsNull, FilterOperator.IsNull)]
    [InlineData(FilterOperators.IsNotNull, FilterOperator.IsNotNull)]
    [InlineData(FilterOperators.NotIn, FilterOperator.NotIn)]
    [InlineData(FilterOperators.Between, FilterOperator.Between)]
    [InlineData(FilterOperators.Any, FilterOperator.Any)]
    [InlineData(FilterOperators.CollectionAll, FilterOperator.All)]
    public void Operator_ShouldMapToSingleMatchingBit(FilterOperators mask, FilterOperator op)
    {
        ((int)mask).Should().Be(1 << (int)op);
        FilterOperators.All.HasFlag(mask).Should().BeTrue();
    }

    [Fact]
    public void FilterOperator_ExistingMembers_ShouldKeepNumbering()
    {
        ((int)FilterOperator.Eq).Should().Be(0);
        ((int)FilterOperator.Ne).Should().Be(1);
        ((int)FilterOperator.Gt).Should().Be(2);
        ((int)FilterOperator.Gte).Should().Be(3);
        ((int)FilterOperator.Lt).Should().Be(4);
        ((int)FilterOperator.Lte).Should().Be(5);
        ((int)FilterOperator.Contains).Should().Be(6);
        ((int)FilterOperator.StartsWith).Should().Be(7);
        ((int)FilterOperator.EndsWith).Should().Be(8);
        ((int)FilterOperator.In).Should().Be(9);
        ((int)FilterOperator.IsNull).Should().Be(10);
        ((int)FilterOperator.IsNotNull).Should().Be(11);
    }

    [Fact]
    public void FilterOperator_NewMembers_ShouldBeAppended()
    {
        ((int)FilterOperator.NotIn).Should().Be(12);
        ((int)FilterOperator.Between).Should().Be(13);
        ((int)FilterOperator.Any).Should().Be(14);
        ((int)FilterOperator.All).Should().Be(15);
    }

    #endregion
}
