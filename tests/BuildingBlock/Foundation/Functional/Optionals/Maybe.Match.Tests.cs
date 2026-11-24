namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeMatchTests
{
    #region Match

    [Theory]
    [InlineData(2, 4)]
    [InlineData(-3, -6)]
    public void Match_WhenSome_InvokesSomeBranch(int value, int expected)
    {
        var result = Maybe<int>.Some(value).Match(
            some => some * 2,
            () => -1);

        result.Should().Be(expected);
    }

    [Fact]
    public void Match_WhenEmpty_InvokesNoneBranch()
    {
        var result = Maybe<int>.Empty().Match(
            some => some,
            () => 99);

        result.Should().Be(99);
    }

    [Fact]
    public void Match_WithNullSome_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Match<int>(null!, () => 0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_WithNullNone_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Match(some => some, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}