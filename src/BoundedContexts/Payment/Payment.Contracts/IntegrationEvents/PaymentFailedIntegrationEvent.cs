using BuildingBlocks.Integration;

namespace Payment.Contracts.IntegrationEvents;

public record PaymentFailedIntegrationEvent(Guid PaymentId, Guid OrderId, string Reason) : IntegrationEvent;
