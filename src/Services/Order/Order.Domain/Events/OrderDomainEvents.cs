using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

/// <summary>
/// Domain Event raised when a new order is created.
/// </summary>
public record OrderCreatedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }

    public OrderCreatedDomainEvent(OrderId orderId, CustomerId customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}

/// <summary>
/// Domain Event raised when an item is added to an order.
/// </summary>
public record OrderItemAddedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public ProductId ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }

    public OrderItemAddedDomainEvent(OrderId orderId, ProductId productId, string productName, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
    }
}

/// <summary>
/// Domain Event raised when an order is submitted.
/// This event triggers the integration with Payment Bounded Context.
/// </summary>
public record OrderSubmittedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }

    public OrderSubmittedDomainEvent(OrderId orderId, CustomerId customerId, decimal totalAmount, string currency)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}

/// <summary>
/// Domain Event raised when an order is paid.
/// </summary>
public record OrderPaidDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public DateTime PaidAt { get; }

    public OrderPaidDomainEvent(OrderId orderId, DateTime paidAt)
    {
        OrderId = orderId;
        PaidAt = paidAt;
    }
}

/// <summary>
/// Domain Event raised when payment fails for an order.
/// </summary>
public record OrderPaymentFailedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public string Reason { get; }

    public OrderPaymentFailedDomainEvent(OrderId orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}

/// <summary>
/// Domain Event raised when an order is shipped.
/// </summary>
public record OrderShippedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }

    public OrderShippedDomainEvent(OrderId orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Domain Event raised when an order is delivered.
/// </summary>
public record OrderDeliveredDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }

    public OrderDeliveredDomainEvent(OrderId orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Domain Event raised when an order is cancelled.
/// </summary>
public record OrderCancelledDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public string Reason { get; }

    public OrderCancelledDomainEvent(OrderId orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}

/// <summary>
/// Domain Event raised when a promotional discount is applied to an order.
/// Carries the computed totals so downstream contexts don't need to recalculate.
/// </summary>
public record PromotionAppliedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public decimal DiscountPercentage { get; }
    public decimal OriginalTotal { get; }
    public decimal DiscountedTotal { get; }
    public string Currency { get; }

    public PromotionAppliedDomainEvent(
        OrderId orderId,
        decimal discountPercentage,
        decimal originalTotal,
        decimal discountedTotal,
        string currency)
    {
        OrderId = orderId;
        DiscountPercentage = discountPercentage;
        OriginalTotal = originalTotal;
        DiscountedTotal = discountedTotal;
        Currency = currency;
    }
}
