namespace BuildingBlock.Domain.Unit.Test.Aggregate;

/// <summary>Tests for <see cref="IAggregateRoot"/> and <see cref="IAggregateRoot{TId}"/>.</summary>
public sealed class AggregateRootInterfaceTests
{
    #region Helper Types

    private sealed record OrderRaised(Guid AggregateId) : DomainEvent;

    private sealed record LineRaised(int AggregateId) : DomainEvent;

    private sealed class OrderAggregate : AggregateRoot
    {
        public OrderAggregate() { }

        public OrderAggregate(Guid id) : base(id) { }

        public void Raise() => RaiseEvent(id => new OrderRaised(id));
    }

    private sealed class LineAggregate : AggregateRoot<int>
    {
        public LineAggregate(int id) : base(id) { }

        public void Raise() => RaiseEvent(id => new LineRaised(id));
    }

    #endregion

    [Fact]
    public void Aggregate_ShouldImplementAggregateRootContract()
    {
        var aggregate = new OrderAggregate();

        aggregate.Should().BeAssignableTo<IAggregateRoot>();
        aggregate.Should().BeAssignableTo<IAggregateRoot<Guid>>();
        new LineAggregate(1).Should().BeAssignableTo<IAggregateRoot<int>>();
    }
}
