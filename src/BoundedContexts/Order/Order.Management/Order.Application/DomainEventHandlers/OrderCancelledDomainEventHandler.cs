using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.Contracts.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderCancelledDomainEventHandler(IEventBus eventBus) : INotificationHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "Cancelled",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
