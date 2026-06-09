using BuildingBlocks.Integration;

namespace Order.IntegrationEvents;

public record OrderItemDiscountSnapshot(Guid ItemId, decimal DiscountAmount);

public record OrderPromotionAppliedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public IReadOnlyList<OrderItemDiscountSnapshot> ItemDiscounts { get; init; } = [];
    public DateTime ModifiedAt { get; init; }
    public int Version { get; init; }
}
