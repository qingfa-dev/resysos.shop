namespace BuildingBlock.Domain.Unit.Test.ValueObjects;

/// <summary>Tests for <see cref="Domain.ValueObjects"/> covering structural equality.</summary>
public sealed class ValueObjectTests
{
    #region Helper Types

    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        public decimal Amount { get; } = amount;

        public string Currency { get; } = currency;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public ValueObject Copy() => GetCopy();
    }

    private sealed class Weight : ValueObject
    {
        public Weight(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }

        public string Currency { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameComponents_ShouldBeEqual()
    {
        var left = new Money(10m, "USD");
        var right = new Money(10m, "USD");

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentComponents_ShouldNotBeEqual()
    {
        var left = new Money(10m, "USD");
        var right = new Money(10m, "EUR");

        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentConcreteTypes_ShouldNotBeEqual()
    {
        var money = new Money(10m, "USD");
        var weight = new Weight(10m, "USD");

        money.Equals(weight).Should().BeFalse();
        (money == weight).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ShouldNotBeEqual()
    {
        var money = new Money(10m, "USD");

        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
        (null == money).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameReference_ShouldBeEqual()
    {
        var money = new Money(10m, "USD");

        money.Equals(money).Should().BeTrue();
    }

    [Fact]
    public void Equals_NonValueObject_ShouldNotBeEqual()
    {
        var money = new Money(10m, "USD");

        money.Equals("not a value object").Should().BeFalse();
    }

    [Fact]
    public void Equals_BoxedValueObject_ShouldUseStructuralEquality()
    {
        object left = new Money(10m, "USD");
        object right = new Money(10m, "USD");

        left.Equals(right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    #endregion

    #region Methods

    [Fact]
    public void ToString_ShouldContainTypeAndComponents()
    {
        var money = new Money(10m, "USD");

        var text = money.ToString();

        text.Should().Contain(nameof(Money));
        text.Should().Contain("10");
        text.Should().Contain("USD");
    }

    [Fact]
    public void GetCopy_ShouldReturnEqualButDistinctInstance()
    {
        var money = new Money(10m, "USD");

        var copy = money.Copy();

        copy.Should().NotBeSameAs(money);
        copy.Should().Be(money);
    }

    #endregion
}
