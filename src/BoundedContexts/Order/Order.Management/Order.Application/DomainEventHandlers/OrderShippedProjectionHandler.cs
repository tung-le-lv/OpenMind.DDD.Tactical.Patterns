using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.Contracts.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderShippedProjectionHandler(IEventBus eventBus) : INotificationHandler<OrderShippedDomainEvent>
{
    public async Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "Shipped",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
