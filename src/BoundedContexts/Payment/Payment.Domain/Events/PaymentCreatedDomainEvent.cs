using BuildingBlocks.Domain;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

public record PaymentCreatedDomainEvent(PaymentId PaymentId, OrderReference OrderId, decimal Amount, string Currency) : DomainEventBase;
