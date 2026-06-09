using BuildingBlocks.Integration;

namespace Order.IntegrationEvents;

/// <summary>
/// Integration Event published when an order is submitted.
/// Used by Payment service to create a payment request.
/// </summary>
public record OrderSubmittedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";

    public OrderSubmittedIntegrationEvent(Guid orderId, Guid customerId, decimal totalAmount, string currency)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}