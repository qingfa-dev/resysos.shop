namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeMonadLawsTests
{
    #region Monad Laws

    [Fact]
    public void Monad_LeftIdentity()
    {
        static Maybe<string> F(int value) => Maybe<string>.Some($"n{value}");

        Maybe<int>.Some(3).Bind(F).Should().BeEquivalentTo(F(3));
    }

    [Fact]
    public void Monad_RightIdentity()
    {
        var maybe = Maybe<int>.Some(3);

        maybe.Bind(value => Maybe<int>.Some(value)).Should().BeEquivalentTo(maybe);
    }

    [Fact]
    public void Monad_Associativity()
    {
        static Maybe<string> F(int value) => Maybe<string>.Some($"n{value}");
        static Maybe<double> G(string value) => Maybe<double>.Some(value.Length);

        var maybe = Maybe<int>.Some(3);

        maybe.Bind(F).Bind(G).Should().BeEquivalentTo(maybe.Bind(value => F(value).Bind(G)));
    }

    #endregion

    #region Functor Laws

    [Fact]
    public void Functor_Identity()
    {
        var maybe = Maybe<int>.Some(3);

        maybe.Map(value => value).Should().BeEquivalentTo(maybe);
    }

    [Fact]
    public void Functor_Composition()
    {
        var maybe = Maybe<int>.Some(3);

        maybe.Map(value => value + 1).Map(value => value * 2)
            .Should().BeEquivalentTo(maybe.Map(value => (value + 1) * 2));
    }

    #endregion
}