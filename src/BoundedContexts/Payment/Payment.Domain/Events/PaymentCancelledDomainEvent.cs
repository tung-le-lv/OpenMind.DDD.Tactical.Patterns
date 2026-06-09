using BuildingBlocks.Domain;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

public record PaymentCancelledDomainEvent(PaymentId PaymentId, OrderReference OrderId, string Reason) : DomainEventBase;
