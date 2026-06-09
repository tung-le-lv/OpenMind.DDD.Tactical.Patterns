using BuildingBlocks.Integration;

namespace Order.Contracts.IntegrationEvents;

public record OrderItemAddedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPriceAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal DiscountAmount { get; init; }
    public bool IsNewItem { get; init; }
    public DateTime ModifiedAt { get; init; }
    public int Version { get; init; }
}
