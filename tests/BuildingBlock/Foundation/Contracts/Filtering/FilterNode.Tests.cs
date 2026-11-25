namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for the filter AST records.</summary>
public sealed class FilterNodeTests
{
    #region Conditions

    [Fact]
    public void Condition_ShouldCarryFieldOperatorAndValue()
    {
        var node = new FilterCondition
        {
            Field = "age", Operator = FilterOperator.Gte, Value = "18"
        };

        node.Field.Should().Be("age");
        node.Operator.Should().Be(FilterOperator.Gte);
        node.Value.Should().Be("18");
        node.CaseInsensitive.Should().BeFalse();
    }

    [Fact]
    public void Condition_Defaults_ShouldMatchDocumentedValues()
    {
        var node = new FilterCondition { Field = "name" };

        node.Operator.Should().Be(FilterConstant.Defaults.Operator);
        node.Value.Should().BeNull();
        node.Values.Should().BeEmpty();
        node.CaseInsensitive.Should().BeFalse();
    }

    [Fact]
    public void Condition_ShouldCarryValuesAndCaseInsensitive()
    {
        var node = new FilterCondition
        {
            Field = "status",
            Operator = FilterOperator.In,
            Values = ["new", "open"],
            CaseInsensitive = true
        };

        node.Values.Should().Equal("new", "open");
        node.CaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public void Condition_ShouldBeAFilterNode()
    {
        FilterNode node = new FilterCondition { Field = "age" };

        node.Should().BeOfType<FilterCondition>();
    }

    #endregion

    #region Groups

    [Fact]
    public void Group_ShouldCarryOperatorAndChildren()
    {
        var node = new FilterGroup
        {
            Operator = FilterGroupOperator.Or,
            Children =
            [
                new FilterCondition { Field = "a", Value = "1" },
                new FilterCondition { Field = "b", Value = "2" }
            ]
        };

        node.Operator.Should().Be(FilterGroupOperator.Or);
        node.Children.Should().HaveCount(2);
        node.Children[1].Should().BeOfType<FilterCondition>()
            .Which.Field.Should().Be("b");
    }

    [Fact]
    public void Group_Defaults_ShouldBeAndWithNoChildren()
    {
        var node = new FilterGroup();

        node.Operator.Should().Be(FilterGroupOperator.And);
        node.Children.Should().BeEmpty();
    }

    [Fact]
    public void GroupOperator_ShouldHaveDocumentedValues()
    {
        ((int)FilterGroupOperator.And).Should().Be(0);
        ((int)FilterGroupOperator.Or).Should().Be(1);
    }

    #endregion

    #region Negation

    [Fact]
    public void Negation_ShouldCarryChild()
    {
        var node = new FilterNegation
        {
            Child = new FilterCondition { Field = "age" }
        };

        node.Child.Should().BeOfType<FilterCondition>()
            .Which.Field.Should().Be("age");
    }

    #endregion
}
