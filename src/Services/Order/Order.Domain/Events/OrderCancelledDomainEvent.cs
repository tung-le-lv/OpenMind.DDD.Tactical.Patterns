using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderCancelledDomainEvent(OrderId OrderId, string Reason) : DomainEventBase;
