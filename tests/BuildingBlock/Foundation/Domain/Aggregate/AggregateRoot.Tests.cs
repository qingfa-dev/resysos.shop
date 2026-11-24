namespace BuildingBlock.Domain.Unit.Test.Aggregate;

/// <summary>Tests for <see cref="AggregateRoot"/> and <see cref="AggregateRoot{TId}"/>.</summary>
public sealed class AggregateRootTests
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
    public void DefaultConstructor_ShouldCreateNonEmptyIdentifier()
    {
        var aggregate = new OrderAggregate();

        aggregate.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ExplicitIdentifier_ShouldBePreserved()
    {
        var id = Guid.NewGuid();

        new OrderAggregate(id).Id.Should().Be(id);
        new LineAggregate(7).Id.Should().Be(7);
    }

    [Fact]
    public void NewAggregate_ShouldHaveNoDomainEvents()
    {
        new OrderAggregate().DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAppendEvent()
    {
        var aggregate = new OrderAggregate();
        var domainEvent = new OrderRaised(aggregate.Id);

        aggregate.AddDomainEvent(domainEvent);

        aggregate.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_Null_ShouldThrow()
    {
        var aggregate = new OrderAggregate();

        var act = () => aggregate.AddDomainEvent(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var aggregate = new OrderAggregate();
        aggregate.AddDomainEvent(new OrderRaised(aggregate.Id));

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseEvent_ShouldUseAggregateIdentifier()
    {
        var id = Guid.NewGuid();
        var aggregate = new OrderAggregate(id);

        aggregate.Raise();

        var domainEvent = aggregate.DomainEvents.Should().ContainSingle().Which;
        domainEvent.Should().BeOfType<OrderRaised>();
        ((OrderRaised)domainEvent).AggregateId.Should().Be(id);
    }

    [Fact]
    public void GenericAggregate_ShouldRaiseTypedEvents()
    {
        var aggregate = new LineAggregate(3);

        aggregate.Raise();

        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LineRaised>()
            .Which.AggregateId.Should().Be(3);
    }
}
