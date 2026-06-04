using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

/// <summary>
/// Carries the computed totals so downstream contexts don't need to recalculate.
/// </summary>
public record PromotionAppliedDomainEvent(
    OrderId OrderId,
    decimal DiscountPercentage,
    decimal OriginalTotal,
    decimal DiscountedTotal,
    string Currency) : DomainEventBase;
