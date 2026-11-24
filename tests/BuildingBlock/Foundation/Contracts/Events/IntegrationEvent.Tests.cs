namespace BuildingBlock.Contracts.Unit.Tests.Events;

/// <summary>Tests for <see cref="IntegrationEvent"/> and <see cref="IIntegrationEvent"/>.</summary>
public sealed class IntegrationEventTests
{
    #region Defaults

    [Fact]
    public void NewEvent_ShouldPopulateIdentityVersionAndType()
    {
        var before = DateTimeOffset.UtcNow;

        var integrationEvent = new OrderPlacedEvent();

        var after = DateTimeOffset.UtcNow;

        integrationEvent.EventId.Should().NotBe(Guid.Empty);
        integrationEvent.CorrelationId.Should().NotBe(Guid.Empty);
        integrationEvent.CausationId.Should().BeNull();
        integrationEvent.OccurredOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        integrationEvent.Version.Should().Be(IntegrationEventConstant.Defaults.Version);
        integrationEvent.EventType.Should().Be(nameof(OrderPlacedEvent));
    }

    [Fact]
    public void NewEvents_ShouldHaveDistinctIdentifiers()
    {
        new OrderPlacedEvent().EventId.Should().NotBe(new OrderPlacedEvent().EventId);
    }

    [Fact]
    public void NewEvent_ShouldStartWithoutMetadata()
    {
        var integrationEvent = new OrderPlacedEvent();

        integrationEvent.Metadata.HasMetadata().Should().BeFalse();
        integrationEvent.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Event_ShouldImplementMarkerContract()
    {
        new OrderPlacedEvent().Should().BeAssignableTo<IIntegrationEvent>();
    }

    #endregion

    #region Enrichment

    [Fact]
    public void WithMetadata_ShouldReturnCopyWithValue()
    {
        var original = new OrderPlacedEvent();

        var enriched = original.WithMetadata<OrderPlacedEvent>("key", "value");

        enriched.Should().NotBeSameAs(original);
        enriched.Metadata.GetMetadata("key").Should().Be("value");
        original.Metadata.HasMetadata().Should().BeFalse();
    }

    [Fact]
    public void WithMetadata_ShouldPreserveEventIdentity()
    {
        var original = new OrderPlacedEvent();

        var enriched = original.WithMetadata<OrderPlacedEvent>("key", "value");

        enriched.EventId.Should().Be(original.EventId);
        enriched.EventType.Should().Be(original.EventType);
    }

    [Fact]
    public void WithCorrelation_ShouldSetCorrelationAndCausation()
    {
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        var enriched = new OrderPlacedEvent().WithCorrelation<OrderPlacedEvent>(
            correlationId,
            causationId);

        enriched.CorrelationId.Should().Be(correlationId);
        enriched.CausationId.Should().Be(causationId);
    }

    [Fact]
    public void WithCorrelation_ShouldDefaultCausationToNull()
    {
        var correlationId = Guid.NewGuid();

        var enriched = new OrderPlacedEvent().WithCorrelation<OrderPlacedEvent>(correlationId);

        enriched.CorrelationId.Should().Be(correlationId);
        enriched.CausationId.Should().BeNull();
    }

    #endregion
}
