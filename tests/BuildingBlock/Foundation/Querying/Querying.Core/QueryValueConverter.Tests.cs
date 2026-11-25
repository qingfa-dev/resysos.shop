namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryValueConverter"/>.</summary>
public sealed class QueryValueConverterTests
{
    #region Helpers

    private enum Color
    {
        Red,
        Green
    }

    #endregion

    #region Supported types

    [Fact]
    public void Convert_String_ShouldPassThrough()
        => QueryValueConverter.Convert("ada", typeof(string), "name").Value.Should().Be("ada");

    [Fact]
    public void Convert_Int_ShouldParse()
        => QueryValueConverter.Convert("42", typeof(int), "age").Value.Should().Be(42);

    [Fact]
    public void Convert_NullableInt_ShouldParseUnderlyingType()
        => QueryValueConverter.Convert("7", typeof(int?), "score").Value.Should().Be(7);

    [Fact]
    public void Convert_Decimal_ShouldParse()
        => QueryValueConverter.Convert("12.5", typeof(decimal), "price").Value.Should().Be(12.5m);

    [Fact]
    public void Convert_Long_ShouldParse()
        => QueryValueConverter.Convert("9000000000", typeof(long), "total").Value.Should().Be(9000000000L);

    [Fact]
    public void Convert_Bool_ShouldParse()
        => QueryValueConverter.Convert("true", typeof(bool), "active").Value.Should().Be(true);

    [Fact]
    public void Convert_Char_ShouldParse()
        => QueryValueConverter.Convert("a", typeof(char), "initial").Value.Should().Be('a');

    [Fact]
    public void Convert_Guid_ShouldParse()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        QueryValueConverter.Convert(id.ToString(), typeof(Guid), "id").Value.Should().Be(id);
    }

    [Fact]
    public void Convert_Enum_ShouldIgnoreCase()
        => QueryValueConverter.Convert("green", typeof(Color), "color").Value.Should().Be(Color.Green);

    [Fact]
    public void Convert_DateTime_ShouldParseRoundTrip()
        => QueryValueConverter.Convert("2024-01-02T03:04:05Z", typeof(DateTime), "createdAt")
            .Value.Should().Be(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc));

    [Fact]
    public void Convert_DateTimeOffset_ShouldParse()
        => QueryValueConverter.Convert("2024-01-02T03:04:05+00:00", typeof(DateTimeOffset), "createdAt")
            .Value.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));

    [Fact]
    public void Convert_DateOnly_ShouldParse()
        => QueryValueConverter.Convert("2024-01-02", typeof(DateOnly), "date")
            .Value.Should().Be(new DateOnly(2024, 1, 2));

    [Fact]
    public void Convert_TimeOnly_ShouldParse()
        => QueryValueConverter.Convert("03:04", typeof(TimeOnly), "time")
            .Value.Should().Be(new TimeOnly(3, 4));

    [Fact]
    public void Convert_TimeSpan_ShouldParse()
        => QueryValueConverter.Convert("01:02:03", typeof(TimeSpan), "duration")
            .Value.Should().Be(new TimeSpan(1, 2, 3));

    #endregion

    #region Failures

    [Fact]
    public void Convert_Null_ShouldReturnRequiredError()
    {
        var result = QueryValueConverter.Convert(null, typeof(int), "age");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.required");
    }

    [Theory]
    [InlineData("abc", typeof(int))]
    [InlineData("not-a-guid", typeof(Guid))]
    [InlineData("maybe", typeof(bool))]
    [InlineData("nope", typeof(Color))]
    [InlineData("999999999999999999999999", typeof(int))]
    public void Convert_Unconvertible_ShouldReturnInvalidError(string raw, Type type)
    {
        var result = QueryValueConverter.Convert(raw, type, "field");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.invalid");
    }

    [Fact]
    public void Convert_UnsupportedType_ShouldReturnInvalidError()
    {
        var result = QueryValueConverter.Convert("x", typeof(Address), "address");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.invalid");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldUseInvariantCulture()
        => QueryValueConverterConstant.Defaults.Culture.Should().Be(CultureInfo.InvariantCulture);

    #endregion
}
