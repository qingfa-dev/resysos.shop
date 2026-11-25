namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="FilterParser"/>.</summary>
public sealed class FilterParserTests
{
    #region Precedence

    [Fact]
    public void Parse_AndOrPrecedence_AndBindsTighterThanOr()
    {
        var node = FilterParser.Parse("a eq 1 or b eq 2 and c eq 3").Value;

        var or = node.Should().BeOfType<FilterGroup>().Which;
        or.Operator.Should().Be(FilterGroupOperator.Or);
        or.Children[1].Should().BeOfType<FilterGroup>()
            .Which.Operator.Should().Be(FilterGroupOperator.And);
    }

    [Fact]
    public void Parse_Parentheses_OverridePrecedence()
    {
        var node = FilterParser.Parse("(a eq 1 or b eq 2) and c eq 3").Value;

        node.Should().BeOfType<FilterGroup>().Which.Operator.Should().Be(FilterGroupOperator.And);
    }

    [Fact]
    public void Parse_SingleCondition_ShouldNotWrapInGroup()
    {
        var node = FilterParser.Parse("a eq 1").Value;

        node.Should().BeOfType<FilterCondition>().Which.Field.Should().Be("a");
    }

    [Fact]
    public void Parse_CommaAsAnd_ShouldBuildAndGroup()
    {
        var node = FilterParser.Parse("a eq 1,b eq 2").Value;

        node.Should().BeOfType<FilterGroup>().Which.Operator.Should().Be(FilterGroupOperator.And);
    }

    #endregion

    #region Negation

    [Fact]
    public void Parse_Not_ShouldWrapInNegation()
    {
        var node = FilterParser.Parse("not a eq 1").Value;

        node.Should().BeOfType<FilterNegation>()
            .Which.Child.Should().BeOfType<FilterCondition>();
    }

    [Fact]
    public void Parse_NotParenthesised_ShouldNegateGroup()
    {
        var node = FilterParser.Parse("not (a eq 1 or b eq 2)").Value;

        node.Should().BeOfType<FilterNegation>()
            .Which.Child.Should().BeOfType<FilterGroup>()
            .Which.Operator.Should().Be(FilterGroupOperator.Or);
    }

    #endregion

    #region Conditions

    [Theory]
    [InlineData("a==1", FilterOperator.Eq, "1")]
    [InlineData("a!=1", FilterOperator.Ne, "1")]
    [InlineData("a<>1", FilterOperator.Ne, "1")]
    [InlineData("a>1", FilterOperator.Gt, "1")]
    [InlineData("a>=1", FilterOperator.Gte, "1")]
    [InlineData("a<1", FilterOperator.Lt, "1")]
    [InlineData("a<=1", FilterOperator.Lte, "1")]
    [InlineData("a@=1", FilterOperator.Contains, "1")]
    [InlineData("a^=1", FilterOperator.StartsWith, "1")]
    [InlineData("a$=1", FilterOperator.EndsWith, "1")]
    public void Parse_SymbolOperator_ShouldMap(string text, FilterOperator expected, string value)
    {
        var node = FilterParser.Parse(text).Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(expected);
        condition.Value.Should().Be(value);
        condition.CaseInsensitive.Should().BeFalse();
    }

    [Theory]
    [InlineData("a eq 1", FilterOperator.Eq)]
    [InlineData("a neq 1", FilterOperator.Ne)]
    [InlineData("a gt 1", FilterOperator.Gt)]
    [InlineData("a gte 1", FilterOperator.Gte)]
    [InlineData("a lt 1", FilterOperator.Lt)]
    [InlineData("a lte 1", FilterOperator.Lte)]
    [InlineData("a contains 1", FilterOperator.Contains)]
    [InlineData("a startswith 1", FilterOperator.StartsWith)]
    [InlineData("a endswith 1", FilterOperator.EndsWith)]
    public void Parse_WordOperator_ShouldMap(string text, FilterOperator expected)
    {
        var node = FilterParser.Parse(text).Value;

        node.Should().BeOfType<FilterCondition>().Which.Operator.Should().Be(expected);
    }

    [Theory]
    [InlineData("a ieq 1", FilterOperator.Eq)]
    [InlineData("a ineq 1", FilterOperator.Ne)]
    [InlineData("a igt 1", FilterOperator.Gt)]
    [InlineData("a igte 1", FilterOperator.Gte)]
    [InlineData("a ilt 1", FilterOperator.Lt)]
    [InlineData("a ilte 1", FilterOperator.Lte)]
    [InlineData("a icontains 1", FilterOperator.Contains)]
    [InlineData("a istartswith 1", FilterOperator.StartsWith)]
    [InlineData("a iendswith 1", FilterOperator.EndsWith)]
    public void Parse_CaseInsensitiveAlias_ShouldFlag(string text, FilterOperator expected)
    {
        var node = FilterParser.Parse(text).Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(expected);
        condition.CaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public void Parse_Between_ShouldCollectBounds()
    {
        var node = FilterParser.Parse("age between 1 and 5").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.Between);
        condition.Values.Should().Equal("1", "5");
    }

    [Fact]
    public void Parse_InList_ShouldCollectValues()
    {
        var node = FilterParser.Parse("status in (a, b)").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.In);
        condition.Values.Should().Equal("a", "b");
    }

    [Fact]
    public void Parse_NotInList_ShouldMap()
    {
        var node = FilterParser.Parse("status notin (a, b)").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.NotIn);
        condition.Values.Should().Equal("a", "b");
    }

    [Fact]
    public void Parse_PipeIn_ShouldSplitValues()
    {
        var node = FilterParser.Parse("status|=new|open").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.In);
        condition.Values.Should().Equal("new", "open");
    }

    [Fact]
    public void Parse_PipeInWithQuotedValues_ShouldSplitValues()
    {
        var node = FilterParser.Parse("status|='new'|'open'").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.In);
        condition.Values.Should().Equal("new", "open");
    }

    [Fact]
    public void Parse_PipeInWithMixedValues_ShouldSplitValues()
    {
        var node = FilterParser.Parse("status|=new|'open'").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.In);
        condition.Values.Should().Equal("new", "open");
    }

    [Fact]
    public void Parse_IsNull_ShouldMap()
    {
        var node = FilterParser.Parse("name isnull").Value;

        node.Should().BeOfType<FilterCondition>().Which.Operator.Should().Be(FilterOperator.IsNull);
    }

    [Fact]
    public void Parse_IsNotNull_ShouldMap()
    {
        var node = FilterParser.Parse("name isnotnull").Value;

        node.Should().BeOfType<FilterCondition>().Which.Operator.Should().Be(FilterOperator.IsNotNull);
    }

    [Fact]
    public void Parse_UnquotedNullEquals_ShouldBecomeIsNull()
    {
        var node = FilterParser.Parse("name==null").Value;

        node.Should().BeOfType<FilterCondition>().Which.Operator.Should().Be(FilterOperator.IsNull);
    }

    [Fact]
    public void Parse_UnquotedNullNotEquals_ShouldBecomeIsNotNull()
    {
        var node = FilterParser.Parse("name!=null").Value;

        node.Should().BeOfType<FilterCondition>().Which.Operator.Should().Be(FilterOperator.IsNotNull);
    }

    [Fact]
    public void Parse_QuotedNull_ShouldBeLiteral()
    {
        var node = FilterParser.Parse("name=='null'").Value;

        var condition = node.Should().BeOfType<FilterCondition>().Which;
        condition.Operator.Should().Be(FilterOperator.Eq);
        condition.Value.Should().Be("null");
    }

    [Fact]
    public void Parse_QuotedValue_ShouldKeepText()
    {
        var node = FilterParser.Parse("name=='a,b'").Value;

        node.Should().BeOfType<FilterCondition>().Which.Value.Should().Be("a,b");
    }

    [Fact]
    public void Parse_DottedField_ShouldBeAccepted()
    {
        var node = FilterParser.Parse("address.city==paris").Value;

        node.Should().BeOfType<FilterCondition>().Which.Field.Should().Be("address.city");
    }

    #endregion

    #region Errors

    [Theory]
    [InlineData("(a eq 1")]
    [InlineData("a eq")]
    [InlineData("and b eq 2")]
    [InlineData("")]
    public void Parse_Malformed_ShouldFailNotThrow(string text)
    {
        var act = () => FilterParser.Parse(text);

        act.Should().NotThrow();
        act().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_InvalidField_ShouldFail()
    {
        var result = FilterParser.Parse("1name==ada");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_FieldWithoutOperator_ShouldFail()
    {
        var result = FilterParser.Parse("name");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_TrailingInput_ShouldFail()
    {
        var result = FilterParser.Parse("a eq 1 b eq 2");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_UnbalancedCloseParen_ShouldFail()
    {
        var result = FilterParser.Parse("a eq 1)");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void Parse_UnterminatedQuote_ShouldPropagateLexError()
    {
        var result = FilterParser.Parse("name eq 'unterminated");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().StartWith("filter.lex.");
    }

    [Fact]
    public void Parse_CollectionOperator_ShouldFail()
    {
        var result = FilterParser.Parse("items any (x eq 1)");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("query.invalid_filter");
    }

    #endregion
}
