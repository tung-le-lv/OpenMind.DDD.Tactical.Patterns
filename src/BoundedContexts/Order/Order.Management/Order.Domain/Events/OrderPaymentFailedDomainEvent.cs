using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public record OrderPaymentFailedDomainEvent(OrderId OrderId, string Reason, int Version) : DomainEventBase;
