using BuildingBlocks.Integration;
using MediatR;
using Payment.Domain.Events;
using Payment.Contracts.IntegrationEvents;

namespace Payment.Application.DomainEventHandlers;

/// <summary>
/// Converts to integration event for Order Bounded Context.
/// </summary>
public class PaymentCompletedDomainEventHandler(IEventBus eventBus) : INotificationHandler<PaymentCompletedDomainEvent>
{
    public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new PaymentCompletedIntegrationEvent(
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Amount,
            notification.CompletedAt);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
