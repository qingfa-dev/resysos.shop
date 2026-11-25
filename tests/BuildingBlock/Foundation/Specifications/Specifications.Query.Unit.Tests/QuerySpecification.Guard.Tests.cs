namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for <see cref="QuerySpecificationGuard"/>.</summary>
public sealed class QuerySpecificationGuardTests
{
    #region Request

    [Fact]
    public void RequestRequired_WithRequest_ShouldSucceed()
    {
        var request = Mapping.Request();

        QuerySpecificationGuard.RequestRequired(request).Value.Should().BeSameAs(request);
    }

    [Fact]
    public void RequestRequired_WithNull_ShouldReturnError()
    {
        QuerySpecificationGuard.RequestRequired(null).Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Request.Required.Error);
    }

    #endregion

    #region Mapper

    [Fact]
    public void MapperRequired_WithMapper_ShouldSucceed()
    {
        var mapper = QuerySpecificationMapper.Instance;

        QuerySpecificationGuard.MapperRequired(mapper).Value.Should().BeSameAs(mapper);
    }

    [Fact]
    public void MapperRequired_WithNull_ShouldReturnError()
    {
        QuerySpecificationGuard.MapperRequired(null).Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Mapper.Required.Error);
    }

    #endregion

    #region Parameters

    [Fact]
    public void ParametersRequired_WithParameters_ShouldSucceed()
    {
        var parameters = new QueryParameters();

        QuerySpecificationGuard.ParametersRequired(parameters).Value.Should().BeSameAs(parameters);
    }

    [Fact]
    public void ParametersRequired_WithNull_ShouldReturnError()
    {
        QuerySpecificationGuard.ParametersRequired(null).Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Parameters.Required.Error);
    }

    #endregion

    #region Paging

    [Fact]
    public void PagingRequired_WithPaging_ShouldSucceed()
    {
        var paging = new PagedRequest();

        QuerySpecificationGuard.PagingRequired(paging).Value.Should().BeSameAs(paging);
    }

    [Fact]
    public void PagingRequired_WithNull_ShouldReturnError()
    {
        QuerySpecificationGuard.PagingRequired(null).Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Paging.Required.Error);
    }

    #endregion

    #region Value Conversion

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    public void ConvertValue_String_ShouldPassThrough(string raw, string expected)
    {
        QuerySpecificationGuard.ConvertValue(raw, typeof(string), "name")
            .Value.Should().Be(expected);
    }

    [Fact]
    public void ConvertValue_Int_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("42", typeof(int), "age")
            .Value.Should().Be(42);
    }

    [Fact]
    public void ConvertValue_NullableInt_ShouldParseUnderlyingType()
    {
        QuerySpecificationGuard.ConvertValue("42", typeof(int?), "score")
            .Value.Should().Be(42);
    }

    [Fact]
    public void ConvertValue_Decimal_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("12.5", typeof(decimal), "salary")
            .Value.Should().Be(12.5m);
    }

    [Fact]
    public void ConvertValue_Bool_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("true", typeof(bool), "verified")
            .Value.Should().Be(true);
    }

    [Fact]
    public void ConvertValue_Guid_ShouldParse()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        QuerySpecificationGuard.ConvertValue(id.ToString(), typeof(Guid), "id")
            .Value.Should().Be(id);
    }

    [Fact]
    public void ConvertValue_Enum_ShouldIgnoreCase()
    {
        QuerySpecificationGuard.ConvertValue("active", typeof(AccountStatus), "status")
            .Value.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void ConvertValue_DateTime_ShouldParseRoundTrip()
    {
        QuerySpecificationGuard.ConvertValue("2024-01-02T03:04:05Z", typeof(DateTime), "createdAt")
            .Value.Should().Be(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc));
    }

    [Fact]
    public void ConvertValue_DateTimeOffset_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue(
                "2024-01-02T03:04:05+00:00",
                typeof(DateTimeOffset),
                "createdAt")
            .Value.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));
    }

    [Fact]
    public void ConvertValue_DateOnly_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("2024-01-02", typeof(DateOnly), "date")
            .Value.Should().Be(new DateOnly(2024, 1, 2));
    }

    [Fact]
    public void ConvertValue_TimeOnly_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("03:04", typeof(TimeOnly), "time")
            .Value.Should().Be(new TimeOnly(3, 4));
    }

    [Fact]
    public void ConvertValue_TimeSpan_ShouldParse()
    {
        QuerySpecificationGuard.ConvertValue("01:02:03", typeof(TimeSpan), "duration")
            .Value.Should().Be(new TimeSpan(1, 2, 3));
    }

    [Fact]
    public void ConvertValue_Null_ShouldReturnRequiredError()
    {
        QuerySpecificationGuard.ConvertValue(null, typeof(int), "age")
            .Errors.Should().ContainSingle()
            .Which.Should().Be(QuerySpecificationConstant.Constraints.Value.Required.Error);
    }

    [Theory]
    [InlineData("abc", typeof(int))]
    [InlineData("not-a-guid", typeof(Guid))]
    [InlineData("maybe", typeof(bool))]
    [InlineData("nope", typeof(AccountStatus))]
    [InlineData("999999999999999999999999", typeof(int))]
    public void ConvertValue_Invalid_ShouldReturnInvalidError(string raw, Type type)
    {
        var result = QuerySpecificationGuard.ConvertValue(raw, type, "field");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.invalid");
    }

    [Fact]
    public void ConvertValue_Invalid_ShouldNotThrow()
    {
        var act = () => QuerySpecificationGuard.ConvertValue("abc", typeof(int), "age");

        act.Should().NotThrow();
    }

    #endregion
}
