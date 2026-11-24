namespace BuildingBlock.Domain.Unit.Test.Events;

/// <summary>Specifications for <see cref="DomainEvent"/> and <see cref="IDomainEvent"/>.</summary>
public sealed class DomainEventSpec
{
    #region Helper Types

    private sealed record TestEvent : DomainEvent;

    #endregion

    #region Defaults

    [Fact]
    public void NewEvent_ShouldPopulateIdentityAndVersion()
    {
        var before = DateTimeOffset.UtcNow;

        var domainEvent = new TestEvent();

        var after = DateTimeOffset.UtcNow;

        domainEvent.EventId.Should().NotBe(Guid.Empty);
        domainEvent.OccurredOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        domainEvent.Version.Should().Be(1);
        domainEvent.EventType.Should().Be(nameof(TestEvent));
    }

    [Fact]
    public void NewEvents_ShouldHaveDistinctIdentifiers()
    {
        new TestEvent().EventId.Should().NotBe(new TestEvent().EventId);
    }

    [Fact]
    public void NewEvent_ShouldStartWithoutMetadata()
    {
        var domainEvent = new TestEvent();

        domainEvent.Metadata.HasMetadata().Should().BeFalse();
        domainEvent.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Event_ShouldImplementMarkerContract()
    {
        new TestEvent().Should().BeAssignableTo<IDomainEvent>();
    }

    #endregion

    #region Metadata

    [Fact]
    public void WithMetadata_ShouldReturnCopyWithValue()
    {
        var original = new TestEvent();

        var enriched = original.WithMetadata<TestEvent>("key", "value");

        enriched.Should().NotBeSameAs(original);
        enriched.Metadata.GetMetadata("key").Should().Be("value");
        original.Metadata.HasMetadata().Should().BeFalse();
    }

    [Fact]
    public void WithMetadata_ShouldPreserveEventIdentity()
    {
        var original = new TestEvent();

        var enriched = original.WithMetadata<TestEvent>("key", "value");

        enriched.EventId.Should().Be(original.EventId);
        enriched.EventType.Should().Be(original.EventType);
    }

    [Fact]
    public void WithCorrelation_ShouldDefaultCausationToCorrelation()
    {
        var correlationId = Guid.NewGuid();

        var enriched = new TestEvent().WithCorrelation<TestEvent>(correlationId);

        enriched.Metadata.GetMetadata(DomainEventConstant.CorrelationId)
            .Should().Be(correlationId);
        enriched.Metadata.GetMetadata(DomainEventConstant.CausationId)
            .Should().Be(correlationId);
    }

    [Fact]
    public void WithCorrelation_ShouldUseExplicitCausation()
    {
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        var enriched = new TestEvent()
            .WithCorrelation<TestEvent>(correlationId, causationId);

        enriched.Metadata.GetMetadata(DomainEventConstant.CorrelationId)
            .Should().Be(correlationId);
        enriched.Metadata.GetMetadata(DomainEventConstant.CausationId)
            .Should().Be(causationId);
    }

    #endregion
}
