using BuildingBlocks.Domain;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

public record PaymentProcessingStartedDomainEvent(
    PaymentId PaymentId,
    OrderReference OrderId,
    long AmountInMinorUnits) : DomainEventBase;
