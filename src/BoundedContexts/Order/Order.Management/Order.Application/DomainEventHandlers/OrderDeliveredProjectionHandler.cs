using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderDeliveredProjectionHandler(IEventBus eventBus) : INotificationHandler<OrderDeliveredDomainEvent>
{
    public async Task Handle(OrderDeliveredDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "Delivered",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
