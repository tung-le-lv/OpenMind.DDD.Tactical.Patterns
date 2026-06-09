using BuildingBlocks.Integration;
using MediatR;
using Payment.Domain.Events;
using Payment.Contracts.IntegrationEvents;

namespace Payment.Application.DomainEventHandlers;

public class PaymentFailedDomainEventHandler(IEventBus eventBus) : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new PaymentFailedIntegrationEvent(
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Reason);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
