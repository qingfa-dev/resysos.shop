namespace BuildingBlock.Domain.Unit.Tests.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeEqualityTests
{
    #region Equality

    [Theory]
    [InlineData(5, 5, true)]
    [InlineData(5, 6, false)]
    public void Equality_ComparesValues(int left, int right, bool expected)
    {
        Maybe<int>.Some(left).Equals(Maybe<int>.Some(right)).Should().Be(expected);
    }

    [Fact]
    public void Equality_SomeVersusEmpty_IsFalse()
    {
        Maybe<int>.Some(0).Equals(Maybe<int>.Empty()).Should().BeFalse();
    }

    [Fact]
    public void Equality_BothEmpty_IsTrue()
    {
        Maybe<int>.Empty().Should().BeEquivalentTo(Maybe<int>.Empty());
    }

    #endregion

    #region GetHashCode

    [Fact]
    public void GetHashCode_SameValue_IsEqual()
    {
        Maybe<int>.Some(5).GetHashCode().Should().Be(Maybe<int>.Some(5).GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_WhenSome_FormatsValue()
    {
        Maybe<int>.Some(5).ToString().Should().Be("Some(5)");
    }

    [Fact]
    public void ToString_WhenEmpty_ReturnsNone()
    {
        Maybe<int>.Empty().ToString().Should().Be("None");
    }

    #endregion
}