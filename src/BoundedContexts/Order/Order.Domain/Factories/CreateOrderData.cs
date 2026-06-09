using Order.Domain.ValueObjects;

namespace Order.Domain.Factories;

/// <summary>
/// Domain-friendly data for creating an Order.
/// This is the domain's language - no external types allowed.
/// </summary>
public record CreateOrderData(
    CustomerId CustomerId,
    Address ShippingAddress,
    string Currency,
    List<OrderItemData> Items,
    string? Notes = null
);

public record OrderItemData(
    ProductId ProductId,
    string ProductName,
    Money UnitPrice,
    int Quantity
);
