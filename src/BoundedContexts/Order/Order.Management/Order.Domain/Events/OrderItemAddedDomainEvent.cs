using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderItemAddedDomainEvent(
    OrderId OrderId,
    Guid ItemId,
    ProductId ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    decimal DiscountAmount,
    bool IsNewItem,
    int Version) : DomainEventBase;
