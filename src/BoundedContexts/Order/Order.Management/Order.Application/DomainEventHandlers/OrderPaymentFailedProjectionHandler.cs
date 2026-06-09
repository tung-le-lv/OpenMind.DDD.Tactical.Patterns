using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.Contracts.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderPaymentFailedProjectionHandler(IEventBus eventBus) : INotificationHandler<OrderPaymentFailedDomainEvent>
{
    public async Task Handle(OrderPaymentFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "PaymentFailed",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
