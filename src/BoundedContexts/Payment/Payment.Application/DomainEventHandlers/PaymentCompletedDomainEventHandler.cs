using BuildingBlocks.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;
using Payment.IntegrationEvents;

namespace Payment.Application.DomainEventHandlers;

/// <summary>
/// Converts to integration event for Order Bounded Context.
/// </summary>
public class PaymentCompletedDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentCompletedDomainEventHandler> logger) : INotificationHandler<PaymentCompletedDomainEvent>
{
    public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} completed for Order {OrderId}",
            notification.PaymentId.Value,
            notification.OrderId.Value);

        var integrationEvent = new PaymentCompletedIntegrationEvent(
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Amount,
            notification.CompletedAt);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
