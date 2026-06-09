using BuildingBlocks.Integration;

namespace Order.IntegrationEvents;

public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public string Currency { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }
    public int Version { get; init; }
}
