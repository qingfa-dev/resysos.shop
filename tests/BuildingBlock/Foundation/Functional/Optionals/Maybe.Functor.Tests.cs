namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeFunctorTests
{
    #region Map

    [Theory]
    [InlineData(2, 4)]
    [InlineData(-3, -6)]
    public void Map_WhenSome_TransformsValue(int value, int expected)
    {
        Maybe<int>.Some(value).Map(number => number * 2).Value.Should().Be(expected);
    }

    [Fact]
    public void Map_WhenEmpty_ReturnsEmpty()
    {
        Maybe<int>.Empty().Map(number => number * 2).HasValue.Should().BeFalse();
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Map<int>(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*maybe.parameter.required*");
    }

    [Fact]
    public void Map_WhenMapperReturnsNull_ReturnsEmpty()
    {
        Maybe<int>.Some(1).Map<string>(_ => null!).HasValue.Should().BeFalse();
    }

    #endregion
}