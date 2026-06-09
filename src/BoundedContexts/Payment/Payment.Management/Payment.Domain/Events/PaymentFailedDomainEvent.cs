using BuildingBlocks.Domain;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

/// <summary>
/// Triggers integration event to notify the Order Bounded Context.
/// </summary>
public record PaymentFailedDomainEvent(PaymentId PaymentId, OrderReference OrderId, string Reason) : DomainEventBase;
