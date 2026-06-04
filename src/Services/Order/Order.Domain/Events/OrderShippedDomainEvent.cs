using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderShippedDomainEvent(OrderId OrderId) : DomainEventBase;
