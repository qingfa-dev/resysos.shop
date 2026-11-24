namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="SortGuard"/>.</summary>
public sealed class SortGuardTests
{
    #region Field

    [Theory]
    [InlineData("name")]
    [InlineData("_name")]
    [InlineData("address.city")]
    [InlineData("total1")]
    public void ValidField_ValidName_ShouldSucceed(string field)
    {
        SortGuard.ValidField(field).Value.Should().Be(field);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1name")]
    [InlineData("na me")]
    [InlineData("na-me")]
    public void ValidField_InvalidName_ShouldReturnError(string field)
    {
        var result = SortGuard.ValidField(field);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.invalid_sort");
    }

    #endregion
}
