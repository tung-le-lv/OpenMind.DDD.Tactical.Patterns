using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

/// <summary>
/// Triggers integration with the Payment Bounded Context.
/// </summary>
public record OrderSubmittedDomainEvent(
    OrderId OrderId,
    CustomerId CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTime SubmittedAt,
    int Version) : DomainEventBase;
