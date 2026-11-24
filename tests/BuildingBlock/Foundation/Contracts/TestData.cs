namespace BuildingBlock.Contracts.Unit.Tests;

#region Events

internal sealed record OrderPlacedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
}

#endregion
