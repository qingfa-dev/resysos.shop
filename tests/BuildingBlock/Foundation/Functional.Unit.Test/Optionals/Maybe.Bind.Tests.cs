namespace BuildingBlock.Domain.Unit.Tests.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeBindTests
{
    #region Bind

    [Theory]
    [InlineData(2, 6)]
    [InlineData(-3, -9)]
    public void Bind_WhenSome_Chains(int value, int expected)
    {
        Maybe<int>.Some(value)
            .Bind(number => Maybe<int>.Some(number * 3))
            .Value.Should().Be(expected);
    }

    [Fact]
    public void Bind_WhenSomeAndBinderReturnsEmpty_ReturnsEmpty()
    {
        Maybe<int>.Some(1).Bind(_ => Maybe<int>.Empty()).HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenEmpty_DoesNotInvokeBinder()
    {
        var invoked = false;

        var result = Maybe<int>.Empty().Bind(value =>
        {
            invoked = true;
            return Maybe<int>.Some(value);
        });

        result.HasValue.Should().BeFalse();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WithNullBinder_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Bind<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}