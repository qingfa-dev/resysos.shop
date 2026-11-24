namespace BuildingBlock.Domain.Unit.Test.Functional.Errors;

using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class ErrorEnumerationTests
{
    [Theory]
    [InlineData(ErrorType.None, -1)]
    [InlineData(ErrorType.Unexpected, 0)]
    [InlineData(ErrorType.Validation, 1)]
    [InlineData(ErrorType.NotFound, 2)]
    [InlineData(ErrorType.Conflict, 3)]
    [InlineData(ErrorType.Unauthorized, 4)]
    [InlineData(ErrorType.Forbidden, 5)]
    [InlineData(ErrorType.BusinessRule, 6)]
    [InlineData(ErrorType.Dependency, 7)]
    [InlineData(ErrorType.Timeout, 8)]
    [InlineData(ErrorType.RateLimited, 9)]
    [InlineData(ErrorType.Cancelled, 10)]
    [InlineData(ErrorType.NotSupported, 11)]
    [InlineData(ErrorType.InvalidState, 12)]
    public void Value_IsExpected(ErrorType type, int expected)
    {
        ((int)type).Should().Be(expected);
    }

    [Fact]
    public void AllValues_AreUnique()
    {
        var values = Enum.GetValues<ErrorType>().Cast<int>().ToArray();

        values.Distinct().Count().Should().Be(values.Length);
    }

    [Fact]
    public void AllValues_AreDefined()
    {
        foreach (var value in Enum.GetValues<ErrorType>())
        {
            Enum.IsDefined(typeof(ErrorType), value).Should().BeTrue();
        }
    }

    [Fact]
    public void None_IsLessThanAllOtherValues()
    {
        var none = (int)ErrorType.None;

        foreach (var value in Enum.GetValues<ErrorType>())
        {
            if (value != ErrorType.None)
            {
                ((int)value).Should().BeGreaterThan(none);
            }
        }
    }
}
