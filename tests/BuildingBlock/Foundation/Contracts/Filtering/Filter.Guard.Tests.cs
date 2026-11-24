namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="FilterGuard"/>.</summary>
public sealed class FilterGuardTests
{
    #region Field

    [Theory]
    [InlineData("name")]
    [InlineData("_name")]
    [InlineData("address.city")]
    [InlineData("name1")]
    public void ValidField_ValidName_ShouldSucceed(string field)
    {
        FilterGuard.ValidField(field, field).Value.Should().Be(field);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1name")]
    [InlineData("na me")]
    [InlineData("na-me")]
    public void ValidField_InvalidName_ShouldReturnError(string field)
    {
        var result = FilterGuard.ValidField(field, field);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.invalid_filter");
    }

    [Fact]
    public void ValidField_ErrorMessage_ShouldIncludeClause()
    {
        var result = FilterGuard.ValidField(string.Empty, "1name==a");

        result.Errors[0].Message.Should().Contain("1name==a");
    }

    #endregion
}
