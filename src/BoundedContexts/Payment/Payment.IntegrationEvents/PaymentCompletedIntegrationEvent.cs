using BuildingBlocks.Integration;

namespace Payment.IntegrationEvents;

public record PaymentCompletedIntegrationEvent(Guid PaymentId, Guid OrderId, decimal Amount, DateTime PaidAt)
    : IntegrationEvent;
