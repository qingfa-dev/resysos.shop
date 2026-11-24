namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeInterfaceTests
{
    #region Interfaces

    [Fact]
    public void Maybe_ImplementsMonadContract()
    {
        var maybe = Maybe<int>.Some(1);

        maybe.Should().BeAssignableTo<IMaybe<int>>();
        maybe.Should().BeAssignableTo<IMaybeMonad<int>>();
        maybe.Should().BeAssignableTo<System.Collections.Generic.IEnumerable<int>>();
    }

    #endregion
}