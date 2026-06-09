using BuildingBlocks.Integration;

namespace Order.Contracts.IntegrationEvents;

public record OrderStatusChangedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public DateTime ModifiedAt { get; init; }
    public int Version { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
