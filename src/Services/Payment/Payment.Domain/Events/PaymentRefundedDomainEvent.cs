using BuildingBlocks.Domain;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

public record PaymentRefundedDomainEvent(PaymentId PaymentId, OrderReference OrderId, decimal Amount, string Reason) : DomainEventBase;
