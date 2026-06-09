using BuildingBlocks.Integration;

namespace Payment.IntegrationEvents;

public record PaymentFailedIntegrationEvent(Guid PaymentId, Guid OrderId, string Reason) : IntegrationEvent;
