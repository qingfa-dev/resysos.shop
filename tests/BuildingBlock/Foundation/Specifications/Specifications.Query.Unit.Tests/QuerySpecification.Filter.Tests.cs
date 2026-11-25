namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for filter mapping.</summary>
public sealed class QuerySpecificationFilterTests
{
    #region Equality

    [Fact]
    public void Eq_Int_ShouldMatchOnlyEqualValues()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.Eq, "30"));

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value).Should().Equal("Ada");
    }

    [Fact]
    public void Ne_Int_ShouldExcludeEqualValues()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.Ne, "30"));

        People.Names(result.Value).Should().Equal("Grace", "Alan", "Noah");
    }

    [Fact]
    public void Eq_String_ShouldMatchExactly()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.Eq, "Ada"));

        People.Names(result.Value).Should().Equal("Ada");
    }

    [Fact]
    public void Eq_Enum_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter("status", FilterOperator.Eq, "Active"));

        People.Names(result.Value).Should().Equal("Ada", "Noah");
    }

    [Fact]
    public void Eq_Guid_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter(
            "id",
            FilterOperator.Eq,
            People.Grace.Id.ToString()));

        People.Names(result.Value).Should().Equal("Grace");
    }

    [Fact]
    public void Eq_NullableInt_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter("score", FilterOperator.Eq, "90"));

        People.Names(result.Value).Should().Equal("Ada");
    }

    [Fact]
    public void FieldName_ShouldBeCaseInsensitive()
    {
        var result = Mapping.Evaluate(Mapping.Filter("AGE", FilterOperator.Eq, "30"));

        People.Names(result.Value).Should().Equal("Ada");
    }

    #endregion

    #region Ordering Comparisons

    [Theory]
    [InlineData(FilterOperator.Gt, "30", "Grace")]
    [InlineData(FilterOperator.Gte, "30", "Ada", "Grace")]
    [InlineData(FilterOperator.Lt, "30", "Alan", "Noah")]
    [InlineData(FilterOperator.Lte, "30", "Ada", "Alan", "Noah")]
    public void OrderingComparisons_Int_ShouldMatch(
        FilterOperator @operator,
        string value,
        params string[] expected)
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", @operator, value));

        People.Names(result.Value).Should().Equal(expected);
    }

    [Fact]
    public void Gt_Decimal_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter("salary", FilterOperator.Gt, "1200"));

        People.Names(result.Value).Should().Equal("Grace");
    }

    [Fact]
    public void Gte_DateTimeOffset_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter(
            "createdAt",
            FilterOperator.Gte,
            People.Start.ToString("O", System.Globalization.CultureInfo.InvariantCulture)));

        People.Names(result.Value).Should().Equal("Ada", "Grace", "Noah");
    }

    [Fact]
    public void Gt_String_ShouldBeUnsupported()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.Gt, "A"));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.operator.unsupported");
    }

    #endregion

    #region Text

    [Fact]
    public void Contains_String_ShouldMatchSubstring()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.Contains, "ra"));

        People.Names(result.Value).Should().Equal("Grace");
    }

    [Fact]
    public void StartsWith_String_ShouldMatchPrefix()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.StartsWith, "A"));

        People.Names(result.Value).Should().Equal("Ada", "Alan");
    }

    [Fact]
    public void EndsWith_String_ShouldMatchSuffix()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.EndsWith, "e"));

        People.Names(result.Value).Should().Equal("Grace");
    }

    [Fact]
    public void Contains_NonString_ShouldBeUnsupported()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.Contains, "3"));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.operator.unsupported");
    }

    #endregion

    #region Sets

    [Fact]
    public void In_String_ShouldMatchAnyValue()
    {
        var result = Mapping.Evaluate(Mapping.Filter(
            "name",
            FilterOperator.In,
            values: ["Ada", "Grace"]));

        People.Names(result.Value).Should().Equal("Ada", "Grace");
    }

    [Fact]
    public void In_Int_ShouldMatchAnyValue()
    {
        var result = Mapping.Evaluate(Mapping.Filter(
            "age",
            FilterOperator.In,
            values: ["17", "45"]));

        People.Names(result.Value).Should().Equal("Grace", "Alan");
    }

    [Fact]
    public void In_NullableInt_ShouldMatchAndIgnoreNulls()
    {
        var result = Mapping.Evaluate(Mapping.Filter(
            "score",
            FilterOperator.In,
            values: ["90", "10"]));

        People.Names(result.Value).Should().Equal("Ada", "Noah");
    }

    [Fact]
    public void In_WithoutValues_ShouldReturnRequiredError()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.In));

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Value.Required.Error);
    }

    #endregion

    #region Null Checks

    [Fact]
    public void IsNull_String_ShouldMatchNull()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.IsNull));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void IsNull_NullableInt_ShouldMatchNull()
    {
        var result = Mapping.Evaluate(Mapping.Filter("score", FilterOperator.IsNull));

        People.Names(result.Value).Should().Equal("Grace");
    }

    [Fact]
    public void IsNull_NonNullableInt_ShouldMatchNothing()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.IsNull));

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void IsNotNull_NonNullableInt_ShouldMatchAll()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.IsNotNull));

        People.Names(result.Value).Should().Equal("Ada", "Grace", "Alan", "Noah");
    }

    [Fact]
    public void IsNull_NestedField_ShouldTreatMissingParentAsNull()
    {
        var result = Mapping.Evaluate(Mapping.Filter("address.country", FilterOperator.IsNull));

        People.Names(result.Value).Should().Equal("Alan", "Noah");
    }

    [Fact]
    public void IsNotNull_NestedField_ShouldRequireParentAndValue()
    {
        var result = Mapping.Evaluate(Mapping.Filter("address.country", FilterOperator.IsNotNull));

        People.Names(result.Value).Should().Equal("Ada", "Grace");
    }

    #endregion

    #region Nested Fields

    [Fact]
    public void Eq_NestedField_ShouldMatch()
    {
        var result = Mapping.Evaluate(Mapping.Filter("address.city", FilterOperator.Eq, "London"));

        People.Names(result.Value).Should().Equal("Ada", "Alan");
    }

    [Fact]
    public void Eq_NestedField_WithNullParent_ShouldNotThrow()
    {
        var act = () => Mapping.Evaluate(Mapping.Filter(
            "address.city",
            FilterOperator.Eq,
            "London"));

        var result = act.Should().NotThrow().Which;

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value).Should().NotContain("Noah");
    }

    #endregion

    #region Composition

    [Fact]
    public void MultipleFilters_ShouldCombineWithAnd()
    {
        var request = Mapping.Request(filters:
        [
            Mapping.Filter("age", FilterOperator.Gte, "18"),
            Mapping.Filter("status", FilterOperator.Eq, "Active")
        ]);

        var result = Mapping.Evaluate(request);

        People.Names(result.Value).Should().Equal("Ada", "Noah");
    }

    [Fact]
    public void NoFilters_ShouldMatchAll()
    {
        var result = Mapping.Evaluate(Mapping.Request());

        People.Names(result.Value).Should().Equal("Ada", "Grace", "Alan", "Noah");
    }

    [Fact]
    public void NullFilterList_ShouldBeTreatedAsEmpty()
    {
        var request = new QueryRequest { Filters = null!, Sorts = null! };

        var result = Mapping.Evaluate(request);

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value).Should().Equal("Ada", "Grace", "Alan", "Noah");
    }

    #endregion

    #region Invalid Input

    [Fact]
    public void UnknownField_ShouldReturnFieldError()
    {
        var result = Mapping.Evaluate(Mapping.Filter("nickname", FilterOperator.Eq, "A"));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.field.unknown");
    }

    [Fact]
    public void EmptyField_ShouldReturnRequiredError()
    {
        var result = Mapping.Evaluate(Mapping.Filter(string.Empty, FilterOperator.Eq, "A"));

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Field.Required.Error);
    }

    [Fact]
    public void MalformedNestedField_ShouldReturnFieldError()
    {
        var result = Mapping.Evaluate(Mapping.Filter("address..city", FilterOperator.Eq, "London"));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.field.unknown");
    }

    [Fact]
    public void UnconvertibleValue_ShouldReturnValueError()
    {
        var result = Mapping.Evaluate(Mapping.Filter("age", FilterOperator.Eq, "abc"));

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.invalid");
    }

    [Fact]
    public void Eq_WithoutValue_ShouldReturnRequiredError()
    {
        var result = Mapping.Evaluate(Mapping.Filter("name", FilterOperator.Eq));

        result.Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Value.Required.Error);
    }

    [Fact]
    public void InvalidFilters_ShouldNotThrow()
    {
        var cases = new[]
        {
            Mapping.Filter("nickname", FilterOperator.Eq, "A"),
            Mapping.Filter("age", FilterOperator.Eq, "abc"),
            Mapping.Filter("name", FilterOperator.In)
        };

        foreach (FilterDescriptor filter in cases)
        {
            var act = () => Mapping.Evaluate(filter);

            act.Should().NotThrow();
        }
    }

    #endregion
}
