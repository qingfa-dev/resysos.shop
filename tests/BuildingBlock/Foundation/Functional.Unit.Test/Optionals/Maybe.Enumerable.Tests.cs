namespace BuildingBlock.Domain.Unit.Tests.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeEnumerableTests
{
    #region Enumerable

    [Fact]
    public void GetEnumerator_WhenSome_YieldsValue()
    {
        var values = Maybe<int>.Some(3).ToArray();

        values.Should().Equal(3);
    }

    [Fact]
    public void GetEnumerator_WhenEmpty_YieldsNothing()
    {
        var values = Maybe<int>.Empty().ToArray();

        values.Should().BeEmpty();
    }

    [Fact]
    public void NonGenericEnumerable_Works()
    {
        System.Collections.IEnumerable some = Maybe<int>.Some(3);

        var values = some.Cast<int>().ToArray();

        values.Should().Equal(3);
    }

    #endregion
}