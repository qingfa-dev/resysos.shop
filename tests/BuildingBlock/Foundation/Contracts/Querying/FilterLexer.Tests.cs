namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="FilterLexer"/>.</summary>
public sealed class FilterLexerTests
{
    #region Tokens

    [Fact]
    public void Tokenize_FieldOperatorValue_ShouldTokenize()
    {
        var tokens = FilterLexer.Tokenize("name eq Ada").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.End);
        tokens.Select(token => token.Text).Should().Equal("name", "eq", "Ada", string.Empty);
    }

    [Fact]
    public void Tokenize_Positions_ShouldBeZeroBased()
    {
        var tokens = FilterLexer.Tokenize("name eq Ada").Value;

        tokens.Select(token => token.Position).Should().Equal(0, 5, 8, 11);
    }

    [Fact]
    public void Tokenize_ShouldAlwaysEndWithEndToken()
    {
        var tokens = FilterLexer.Tokenize("name eq Ada").Value;

        tokens[^1].Kind.Should().Be(FilterTokenKind.End);
        tokens[^1].Text.Should().BeEmpty();
        tokens[^1].Position.Should().Be("name eq Ada".Length);
    }

    #endregion

    #region Symbols

    [Theory]
    [InlineData("name==ada", "==")]
    [InlineData("name!=ada", "!=")]
    [InlineData("name<>ada", "<>")]
    [InlineData("name>=ada", ">=")]
    [InlineData("name<=ada", "<=")]
    [InlineData("name>ada", ">")]
    [InlineData("name<ada", "<")]
    [InlineData("name@=ada", "@=")]
    [InlineData("name^=ada", "^=")]
    [InlineData("name$=ada", "$=")]
    [InlineData("status|=new", "|=")]
    public void Tokenize_Symbol_ShouldTokenizeAsSymbol(string text, string symbol)
    {
        var tokens = FilterLexer.Tokenize(text).Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Symbol);
        tokens[1].Text.Should().Be(symbol);
    }

    [Fact]
    public void Tokenize_TwoCharacterSymbol_ShouldPreferLongestMatch()
    {
        var tokens = FilterLexer.Tokenize("a>=1").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Symbol);
        tokens[1].Text.Should().Be(">=");
    }

    #endregion

    #region WordOperators

    [Theory]
    [InlineData("eq")]
    [InlineData("neq")]
    [InlineData("gt")]
    [InlineData("gte")]
    [InlineData("lt")]
    [InlineData("lte")]
    [InlineData("contains")]
    [InlineData("startswith")]
    [InlineData("endswith")]
    [InlineData("in")]
    [InlineData("notin")]
    [InlineData("between")]
    [InlineData("isnull")]
    [InlineData("isnotnull")]
    [InlineData("any")]
    [InlineData("all")]
    [InlineData("and")]
    [InlineData("or")]
    [InlineData("not")]
    public void Tokenize_WordOperator_ShouldTokenizeAsWord(string word)
    {
        var tokens = FilterLexer.Tokenize($"name {word} ada").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Word);
        tokens[1].Text.Should().Be(word);
    }

    [Theory]
    [InlineData("ieq")]
    [InlineData("ineq")]
    [InlineData("igt")]
    [InlineData("igte")]
    [InlineData("ilt")]
    [InlineData("ilte")]
    [InlineData("icontains")]
    [InlineData("istartswith")]
    [InlineData("iendswith")]
    public void Tokenize_CaseInsensitiveAlias_ShouldTokenizeAsWord(string word)
    {
        var tokens = FilterLexer.Tokenize($"name {word} ada").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Word);
    }

    [Fact]
    public void Tokenize_WordOperator_ShouldBeCaseInsensitive()
    {
        var tokens = FilterLexer.Tokenize("name EQ Ada").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Word);
        tokens[1].Text.Should().Be("EQ");
    }

    #endregion

    #region Values

    [Fact]
    public void Tokenize_QuotedValue_KeepsCommaInside()
    {
        var tokens = FilterLexer.Tokenize("name eq 'a,b'").Value;

        tokens.Should().ContainSingle(token => token.Kind == FilterTokenKind.QuotedValue && token.Text == "a,b");
    }

    [Fact]
    public void Tokenize_QuotedValue_ShouldStripQuotes()
    {
        var tokens = FilterLexer.Tokenize("name eq 'ada'").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.QuotedValue);
        tokens[2].Text.Should().Be("ada");
    }

    [Fact]
    public void Tokenize_QuotedValue_ShouldUnescapeDoubledQuote()
    {
        var tokens = FilterLexer.Tokenize("name eq 'it''s'").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.QuotedValue);
        tokens[2].Text.Should().Be("it's");
    }

    [Fact]
    public void Tokenize_QuotedValue_ShouldAllowEmpty()
    {
        var tokens = FilterLexer.Tokenize("name eq ''").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.QuotedValue);
        tokens[2].Text.Should().BeEmpty();
    }

    [Fact]
    public void Tokenize_QuotedValue_PositionShouldPointAtOpeningQuote()
    {
        var tokens = FilterLexer.Tokenize("name eq 'ada'").Value;

        tokens[2].Position.Should().Be(8);
    }

    [Fact]
    public void Tokenize_UnquotedValue_ShouldStopAtComma()
    {
        var tokens = FilterLexer.Tokenize("name==ada,total>=10").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be("ada");
    }

    #endregion

    #region Structure

    [Fact]
    public void Tokenize_ParenthesesAndComma_ShouldTokenize()
    {
        var tokens = FilterLexer.Tokenize("(a eq 1, b eq 2)").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.OpenParen,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.Comma,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.CloseParen,
            FilterTokenKind.End);
    }

    #endregion

    #region Classification

    [Fact]
    public void Tokenize_BooleanOperator_ShouldReturnToFieldPosition()
    {
        var tokens = FilterLexer.Tokenize("a eq 1 and b eq 2").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.Word,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_Not_ShouldReturnToFieldPosition()
    {
        var tokens = FilterLexer.Tokenize("not a eq 1").Value;

        tokens[0].Kind.Should().Be(FilterTokenKind.Word);
        tokens[0].Text.Should().Be("not");
        tokens[1].Kind.Should().Be(FilterTokenKind.Field);
        tokens[1].Text.Should().Be("a");
    }

    [Fact]
    public void Tokenize_Any_ShouldTreatInnerIdentifierAsField()
    {
        var tokens = FilterLexer.Tokenize("items any (x eq 1)").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.OpenParen,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.CloseParen,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_In_ShouldTreatInnerIdentifiersAsValues()
    {
        var tokens = FilterLexer.Tokenize("status in (a, b)").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.OpenParen,
            FilterTokenKind.Value,
            FilterTokenKind.Comma,
            FilterTokenKind.Value,
            FilterTokenKind.CloseParen,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_Between_ShouldTreatSecondOperandAsValue()
    {
        var tokens = FilterLexer.Tokenize("age between 1 and 5").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.End);
        tokens[4].Text.Should().Be("5");
    }

    [Fact]
    public void Tokenize_NullCheck_ShouldStayInOperatorPosition()
    {
        var tokens = FilterLexer.Tokenize("name isnull and age isnotnull").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Word,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_NestedGroups_ShouldTrackValueListContext()
    {
        var tokens = FilterLexer.Tokenize("(a in (1, 2) or b eq 3)").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.OpenParen,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.OpenParen,
            FilterTokenKind.Value,
            FilterTokenKind.Comma,
            FilterTokenKind.Value,
            FilterTokenKind.CloseParen,
            FilterTokenKind.Word,
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.Value,
            FilterTokenKind.CloseParen,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_InListWithQuotedValues_ShouldTokenize()
    {
        var tokens = FilterLexer.Tokenize("status in ('new', 'open')").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Word,
            FilterTokenKind.OpenParen,
            FilterTokenKind.QuotedValue,
            FilterTokenKind.Comma,
            FilterTokenKind.QuotedValue,
            FilterTokenKind.CloseParen,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_DottedField_ShouldTokenizeAsField()
    {
        var tokens = FilterLexer.Tokenize("address.city eq paris").Value;

        tokens[0].Kind.Should().Be(FilterTokenKind.Field);
        tokens[0].Text.Should().Be("address.city");
    }

    [Fact]
    public void Tokenize_WordSpellingInValuePosition_ShouldBeValue()
    {
        var tokens = FilterLexer.Tokenize("name eq and").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be("and");
    }

    #endregion

    #region BackCompat

    [Fact]
    public void Tokenize_LegacySymbolDialect_ShouldTokenize()
    {
        var tokens = FilterLexer.Tokenize("name@=a,total>=10").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.Symbol,
            FilterTokenKind.Value,
            FilterTokenKind.Comma,
            FilterTokenKind.Field,
            FilterTokenKind.Symbol,
            FilterTokenKind.Value,
            FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_LegacyInValues_ShouldKeepPipeInsideValue()
    {
        var tokens = FilterLexer.Tokenize("status|=new|open").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Symbol);
        tokens[1].Text.Should().Be("|=");
        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be("new|open");
    }

    [Fact]
    public void Tokenize_LegacyInQuotedValues_ShouldJoinPipeList()
    {
        var tokens = FilterLexer.Tokenize("status|='new'|'open'").Value;

        tokens[1].Kind.Should().Be(FilterTokenKind.Symbol);
        tokens[1].Text.Should().Be("|=");
        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be("new|open");
    }

    [Fact]
    public void Tokenize_LegacyInMixedValues_ShouldJoinPipeList()
    {
        var tokens = FilterLexer.Tokenize("status|=new|'open'").Value;

        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be("new|open");
    }

    [Theory]
    [InlineData("id==3fa85f64-5717-4562-b3fc-2c963f66afa6", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData("created>=2024-01-01", "2024-01-01")]
    [InlineData("time==12:30", "12:30")]
    [InlineData("created>=2024-01-01T00:00:00Z", "2024-01-01T00:00:00Z")]
    [InlineData("path==a/b", "a/b")]
    [InlineData("name==a-b:c+d/e@f#g%h&i*j~k?l=m", "a-b:c+d/e@f#g%h&i*j~k?l=m")]
    public void Tokenize_LegacyUnquotedValue_ShouldStaySingleValue(string text, string value)
    {
        var tokens = FilterLexer.Tokenize(text).Value;

        tokens.Should().ContainSingle(token => token.Kind == FilterTokenKind.Value);
        tokens[2].Kind.Should().Be(FilterTokenKind.Value);
        tokens[2].Text.Should().Be(value);
    }

    [Fact]
    public void Tokenize_FieldCharset_ShouldNotWiden()
    {
        var result = FilterLexer.Tokenize("a-b==1");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("filter.lex.unexpected_character");
    }

    [Fact]
    public void Tokenize_FieldWithoutOperator_ShouldStillLex()
    {
        var tokens = FilterLexer.Tokenize("name").Value;

        tokens.Select(token => token.Kind).Should().Equal(
            FilterTokenKind.Field,
            FilterTokenKind.End);
    }

    #endregion

    #region Errors

    [Theory]
    [InlineData("name eq 'unterminated")]
    [InlineData("name eq \\")]
    public void Tokenize_Malformed_ShouldFailNotThrow(string text)
    {
        var act = () => FilterLexer.Tokenize(text);

        act.Should().NotThrow();
        act().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Tokenize_UnterminatedQuote_ShouldCarryLexError()
    {
        var result = FilterLexer.Tokenize("name eq 'unterminated");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("filter.lex.unterminated_quote");
    }

    [Fact]
    public void Tokenize_StrayBackslash_ShouldCarryLexError()
    {
        var result = FilterLexer.Tokenize("name eq \\");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("filter.lex.unexpected_character");
    }

    [Theory]
    [InlineData("name??ada")]
    [InlineData("name = ada")]
    [InlineData("name ! ada")]
    [InlineData("name | ada")]
    public void Tokenize_UnknownCharacter_ShouldFailWithLexError(string text)
    {
        var result = FilterLexer.Tokenize(text);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().StartWith("filter.lex.");
    }

    #endregion

    #region Empty

    [Fact]
    public void Tokenize_Null_ShouldReturnOnlyEnd()
    {
        var result = FilterLexer.Tokenize(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Kind.Should().Be(FilterTokenKind.End);
        result.Value[0].Position.Should().Be(0);
    }

    [Fact]
    public void Tokenize_Empty_ShouldReturnOnlyEnd()
    {
        var result = FilterLexer.Tokenize(string.Empty);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].Kind.Should().Be(FilterTokenKind.End);
    }

    [Fact]
    public void Tokenize_Whitespace_ShouldReturnOnlyEnd()
    {
        var result = FilterLexer.Tokenize("   ");

        result.Value.Should().ContainSingle();
        result.Value[0].Kind.Should().Be(FilterTokenKind.End);
        result.Value[0].Position.Should().Be(3);
    }

    #endregion

    #region FilterToken

    [Fact]
    public void FilterToken_ShouldCarryKindTextAndPosition()
    {
        var token = new FilterToken(FilterTokenKind.Field, "name", 3);

        token.Kind.Should().Be(FilterTokenKind.Field);
        token.Text.Should().Be("name");
        token.Position.Should().Be(3);
    }

    [Fact]
    public void FilterTokenKind_ShouldKeepDeclarationOrder()
    {
        ((int)FilterTokenKind.Field).Should().Be(0);
        ((int)FilterTokenKind.Word).Should().Be(1);
        ((int)FilterTokenKind.Symbol).Should().Be(2);
        ((int)FilterTokenKind.Value).Should().Be(3);
        ((int)FilterTokenKind.QuotedValue).Should().Be(4);
        ((int)FilterTokenKind.OpenParen).Should().Be(5);
        ((int)FilterTokenKind.CloseParen).Should().Be(6);
        ((int)FilterTokenKind.Comma).Should().Be(7);
        ((int)FilterTokenKind.End).Should().Be(8);
    }

    #endregion
}
