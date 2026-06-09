using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderItemAddedDomainEvent(OrderId OrderId, ProductId ProductId, string ProductName, int Quantity) : DomainEventBase;
