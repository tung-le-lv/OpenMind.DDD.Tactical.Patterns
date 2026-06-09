using BuildingBlocks.Integration;

namespace Payment.Contracts.IntegrationEvents;

public record PaymentCompletedIntegrationEvent(Guid PaymentId, Guid OrderId, decimal Amount, DateTime PaidAt)
    : IntegrationEvent;
