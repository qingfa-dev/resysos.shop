using BuildingBlock.Domain.StronglyTypedId;

namespace BuildingBlock.Domain.Unit.Test.StronglyTypedIds;

/// <summary>Specifications for <see cref="StronglyTypedId{TValue}"/> and its Guid and string bases.</summary>
public sealed class StronglyTypedIdSpec
{
    #region Helper Types

    private sealed record OrderId : StronglyTypedId<Guid>
    {
        public OrderId(Guid value) : base(value) { }
    }

    private sealed record CustomerId : StronglyTypedId<Guid>
    {
        public CustomerId(Guid value) : base(value) { }
    }

    private sealed record SequenceId : StronglyTypedId<int>
    {
        public SequenceId(int value) : base(value) { }
    }

    private sealed record ProductCode : StringId
    {
        public ProductCode(string value) : base(value) { }
    }

    private sealed record CatalogId : GuidId
    {
        public CatalogId(Guid value) : base(value) { }
    }

    #endregion

    #region Construction

    [Fact]
    public void Constructor_ShouldPreserveValue()
    {
        var value = Guid.NewGuid();

        var id = new OrderId(value);

        id.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_NullString_ShouldThrow()
    {
        var act = () => new ProductCode(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankString_ShouldThrow(string value)
    {
        var act = () => new ProductCode(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptyGuid_ShouldThrow()
    {
        var act = () => new OrderId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_DefaultInt_ShouldThrow()
    {
        var act = () => new SequenceId(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GuidId_NewValue_ShouldReturnDistinctNonEmptyValues()
    {
        var first = CatalogId.NewValue();
        var second = CatalogId.NewValue();

        first.Should().NotBe(Guid.Empty);
        first.Should().NotBe(second);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameTypeAndValue_ShouldBeEqual()
    {
        var value = Guid.NewGuid();
        var left = new OrderId(value);
        var right = new OrderId(value);

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_ShouldNotBeEqual()
    {
        var left = new OrderId(Guid.NewGuid());
        var right = new OrderId(Guid.NewGuid());

        left.Should().NotBe(right);
    }

    [Fact]
    public void Equals_DifferentIdTypesWithSameValue_ShouldNotBeEqual()
    {
        var value = Guid.NewGuid();
        var orderId = new OrderId(value);
        var customerId = new CustomerId(value);

        orderId.Equals(customerId).Should().BeFalse();
    }

    #endregion

    #region Comparison and formatting

    [Fact]
    public void CompareTo_ShouldCompareUnderlyingValues()
    {
        var lower = new SequenceId(1);
        var higher = new SequenceId(2);

        lower.CompareTo(higher).Should().BeNegative();
        higher.CompareTo(lower).Should().BePositive();
        lower.CompareTo(new SequenceId(1)).Should().Be(0);
    }

    [Fact]
    public void CompareTo_Null_ShouldReturnPositive()
    {
        var id = new SequenceId(1);

        id.CompareTo(null).Should().BePositive();
    }

    [Fact]
    public void ToString_ShouldReturnUnderlyingValue()
    {
        var value = Guid.NewGuid();

        new OrderId(value).ToString().Should().Be(value.ToString());
        new ProductCode("sku-1").ToString().Should().Be("sku-1");
    }

    #endregion
}
