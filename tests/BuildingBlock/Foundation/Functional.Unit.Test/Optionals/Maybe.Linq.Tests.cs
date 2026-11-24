namespace BuildingBlock.Domain.Unit.Tests.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeLinqTests
{
    #region Select

    [Theory]
    [InlineData(2, 4)]
    [InlineData(-3, -6)]
    public void Select_WhenSome_MapsValue(int value, int expected)
    {
        Maybe<int>.Some(value).Select(number => number * 2).Value.Should().Be(expected);
    }

    [Fact]
    public void Select_WhenEmpty_ReturnsEmpty()
    {
        Maybe<int>.Empty().Select(number => number * 2).HasValue.Should().BeFalse();
    }

    #endregion

    #region SelectMany (Bind)

    [Fact]
    public void SelectMany_WithBinder_Chains()
    {
        Maybe<int>.Some(2)
            .SelectMany(number => Maybe<string>.Some($"n{number}"))
            .Value.Should().Be("n2");
    }

    [Fact]
    public void SelectMany_WithProjector_Projects()
    {
        var result = Maybe<int>.Some(2).SelectMany(
            number => Maybe<int>.Some(number * 3),
            (value, intermediate) => value + intermediate);

        result.Value.Should().Be(8);
    }

    [Fact]
    public void SelectMany_WithProjector_WhenEmpty_ReturnsEmpty()
    {
        var result = Maybe<int>.Empty().SelectMany(
            number => Maybe<int>.Some(number),
            (value, intermediate) => value + intermediate);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void SelectMany_WithProjector_WhenBinderEmpty_ReturnsEmpty()
    {
        var result = Maybe<int>.Some(2).SelectMany(
            _ => Maybe<int>.Empty(),
            (value, intermediate) => value + intermediate);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void SelectMany_WithProjectorAndNullBinder_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).SelectMany<int, int, int>(
            null!,
            (value, intermediate) => value + intermediate);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectMany_WithProjectorAndNullProjector_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).SelectMany<int, int, int>(
            value => Maybe<int>.Some(value),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Where

    [Theory]
    [InlineData(2, true)]
    [InlineData(-2, false)]
    public void Where_WhenSome_FiltersByPredicate(int value, bool expectedHasValue)
    {
        Maybe<int>.Some(value).Where(number => number > 0).HasValue.Should().Be(expectedHasValue);
    }

    [Fact]
    public void Where_WhenEmpty_ReturnsEmpty()
    {
        Maybe<int>.Empty().Where(number => number > 0).HasValue.Should().BeFalse();
    }

    [Fact]
    public void Where_WithNullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Where(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Query Syntax

    [Fact]
    public void QuerySyntax_Select_Works()
    {
        var result = from value in Maybe<int>.Some(5)
                     select value * 2;

        result.Value.Should().Be(10);
    }

    [Fact]
    public void QuerySyntax_Where_Works()
    {
        var result = from value in Maybe<int>.Some(5)
                     where value > 0
                     select value * 2;

        result.Value.Should().Be(10);
    }

    [Fact]
    public void QuerySyntax_MultipleFrom_Works()
    {
        var result = from x in Maybe<int>.Some(2)
                     from y in Maybe<int>.Some(3)
                     select x + y;

        result.Value.Should().Be(5);
    }

    [Fact]
    public void QuerySyntax_MultipleFrom_WithEmpty_ReturnsEmpty()
    {
        var result = from x in Maybe<int>.Empty()
                     from y in Maybe<int>.Some(3)
                     select x + y;

        result.HasValue.Should().BeFalse();
    }

    #endregion
}