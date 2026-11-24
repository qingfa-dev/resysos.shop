using BuildingBlock.Domain.Entities;

namespace BuildingBlock.Domain.Unit.Test.Entities;

/// <summary>Tests for <see cref="Entity{TId}"/>.</summary>
public sealed class EntityTests
{
    #region Helper Types

    private sealed class Order : Entity<Guid>
    {
        public Order(Guid id) : base(id) { }
    }

    private sealed class Invoice : Entity<Guid>
    {
        public Invoice(Guid id) : base(id) { }
    }

    private sealed class Line : Entity<int>
    {
        public Line(int id) : base(id) { }
    }

    private sealed class AuditedOrder : Entity<Guid>, IAuditable
    {
        public AuditedOrder(Guid id) : base(id) { }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? ModifiedAtUtc { get; set; }

        public string? ModifiedBy { get; set; }
    }

    #endregion

    [Fact]
    public void Constructor_ShouldPreserveIdentifier()
    {
        var id = Guid.NewGuid();

        new Order(id).Id.Should().Be(id);
        new Line(42).Id.Should().Be(42);
    }

    [Fact]
    public void Equals_SameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var left = new Order(id);
        var right = new Order(id);

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentId_ShouldNotBeEqual()
    {
        var left = new Order(Guid.NewGuid());
        var right = new Order(Guid.NewGuid());

        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentEntityTypesWithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var order = new Order(id);
        var invoice = new Invoice(id);

        order.Equals(invoice).Should().BeTrue();
        (order == invoice).Should().BeTrue();
    }

    [Fact]
    public void Equals_Null_ShouldNotBeEqual()
    {
        var order = new Order(Guid.NewGuid());

        order.Equals(null).Should().BeFalse();
        (order == null).Should().BeFalse();
        (null == order).Should().BeFalse();
    }

    [Fact]
    public void Equals_NonEntity_ShouldNotBeEqual()
    {
        var order = new Order(Guid.NewGuid());

        order.Equals("not an entity").Should().BeFalse();
    }

    [Fact]
    public void Equals_SameReference_ShouldBeEqual()
    {
        var order = new Order(Guid.NewGuid());

        order.Equals(order).Should().BeTrue();
    }
}
