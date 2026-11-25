namespace BuildingBlock.Domain.Unit.Test.Events;

/// <summary>Tests for <see cref="IHasDomainEvents"/> and <see cref="EntityWithDomainEvents"/>.</summary>
public sealed class HasDomainEventsInterfaceTests
{
    #region Helper Types

    private sealed record OrderCreated(Guid OrderId) : DomainEvent;

    private sealed record OrderConfirmed(Guid OrderId) : DomainEvent;

    private sealed class OrderLine : EntityWithDomainEvents
    {
        public void Raise(Guid orderId) => RaiseEvent(() => new OrderCreated(orderId));
    }

    #endregion

    [Fact]
    public void Entity_ShouldImplementHasDomainEventsContract()
    {
        new OrderLine().Should().BeAssignableTo<IHasDomainEvents>();
    }

    [Fact]
    public void NewEntity_ShouldHaveNoDomainEvents()
    {
        new OrderLine().DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAppendEventsInTheOrderTheyWereAdded()
    {
        var orderId = Guid.NewGuid();
        var entity = new OrderLine();
        var created = new OrderCreated(orderId);
        var confirmed = new OrderConfirmed(orderId);

        entity.AddDomainEvent(created);
        entity.AddDomainEvent(confirmed);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents[0].Should().BeSameAs(created);
        entity.DomainEvents[1].Should().BeSameAs(confirmed);
    }

    [Fact]
    public void AddDomainEvent_Null_ShouldThrow()
    {
        var entity = new OrderLine();

        var act = () => entity.AddDomainEvent(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainEvents_ShouldNotExposeMutableCollection()
    {
        var entity = new OrderLine();
        entity.AddDomainEvent(new OrderCreated(Guid.NewGuid()));

        var events = (IList<IDomainEvent>)entity.DomainEvents;

        events.IsReadOnly.Should().BeTrue();
        var act = () => events.Add(new OrderConfirmed(Guid.NewGuid()));
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var entity = new OrderLine();
        var orderId = Guid.NewGuid();
        entity.AddDomainEvent(new OrderCreated(orderId));
        entity.AddDomainEvent(new OrderConfirmed(orderId));

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenEmpty_ShouldNotThrow()
    {
        var entity = new OrderLine();

        var act = () => entity.ClearDomainEvents();

        act.Should().NotThrow();
    }

    [Fact]
    public void ClearDomainEvents_ShouldAllowAddingEventsAfterwards()
    {
        var entity = new OrderLine();
        entity.AddDomainEvent(new OrderCreated(Guid.NewGuid()));
        entity.ClearDomainEvents();

        var domainEvent = new OrderConfirmed(Guid.NewGuid());
        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void RaiseEvent_ShouldAddEventProducedByFactory()
    {
        var orderId = Guid.NewGuid();
        var entity = new OrderLine();

        entity.Raise(orderId);

        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreated>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void SeparateEntities_ShouldNotShareDomainEvents()
    {
        var first = new OrderLine();
        var second = new OrderLine();

        first.AddDomainEvent(new OrderCreated(Guid.NewGuid()));

        first.DomainEvents.Should().ContainSingle();
        second.DomainEvents.Should().BeEmpty();
    }
}
