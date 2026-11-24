namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeAlternativeTests
{
    #region Or

    [Fact]
    public void Or_WhenSome_ReturnsThis()
    {
        var some = Maybe<int>.Some(1);

        some.Or(Maybe<int>.Some(2)).Should().BeEquivalentTo(some);
    }

    [Fact]
    public void Or_WhenEmpty_ReturnsAlternative()
    {
        Maybe<int>.Empty().Or(Maybe<int>.Some(2)).Value.Should().Be(2);
    }

    [Fact]
    public void Or_WhenBothEmpty_ReturnsEmpty()
    {
        Maybe<int>.Empty().Or(Maybe<int>.Empty()).HasValue.Should().BeFalse();
    }

    #endregion

    #region OrElse

    [Fact]
    public void OrElse_WhenSome_DoesNotInvokeFactory()
    {
        var invoked = false;
        var some = Maybe<int>.Some(1);

        var result = some.OrElse(() =>
        {
            invoked = true;
            return Maybe<int>.Some(2);
        });

        result.Should().BeEquivalentTo(some);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void OrElse_WhenEmpty_InvokesFactory()
    {
        Maybe<int>.Empty().OrElse(() => Maybe<int>.Some(2)).Value.Should().Be(2);
    }

    [Fact]
    public void OrElse_WhenFactoryReturnsEmpty_ReturnsEmpty()
    {
        Maybe<int>.Empty().OrElse(() => Maybe<int>.Empty()).HasValue.Should().BeFalse();
    }

    [Fact]
    public void OrElse_WithNullFactory_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<int>.Empty().OrElse(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}