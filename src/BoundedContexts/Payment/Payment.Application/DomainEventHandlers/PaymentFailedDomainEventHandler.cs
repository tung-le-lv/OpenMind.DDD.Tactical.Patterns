using BuildingBlocks.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;
using Payment.IntegrationEvents;

namespace Payment.Application.DomainEventHandlers;

public class PaymentFailedDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentFailedDomainEventHandler> logger) : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Payment {PaymentId} failed for Order {OrderId}. Reason: {Reason}",
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Reason);

        var integrationEvent = new PaymentFailedIntegrationEvent(
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Reason);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
