namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeTapTests
{
    #region Tap

    [Fact]
    public void Tap_WhenSome_InvokesActionAndReturnsSame()
    {
        var seen = 0;
        var some = Maybe<int>.Some(4);

        var result = some.Tap(value => seen = value);

        result.Should().BeEquivalentTo(some);
        seen.Should().Be(4);
    }

    [Fact]
    public void Tap_WhenEmpty_DoesNotInvokeAction()
    {
        var invoked = false;

        Maybe<int>.Empty().Tap(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Tap_WithNullAction_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Some(1).Tap(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}