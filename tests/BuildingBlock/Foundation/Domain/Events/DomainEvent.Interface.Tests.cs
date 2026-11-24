namespace BuildingBlock.Domain.Unit.Test.Events;

/// <summary>Tests for <see cref="IDomainEvent"/>.</summary>
public sealed class DomainEventInterfaceTests
{
    #region Helper Types

    private sealed record TestEvent : DomainEvent;

    #endregion

    [Fact]
    public void Event_ShouldImplementMarkerContract()
    {
        new TestEvent().Should().BeAssignableTo<IDomainEvent>();
    }
}
