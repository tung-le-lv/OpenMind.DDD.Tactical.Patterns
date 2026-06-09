using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.Contracts.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderPaidProjectionHandler(IEventBus eventBus) : INotificationHandler<OrderPaidDomainEvent>
{
    public async Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "Paid",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version,
            PaidAt = notification.PaidAt
        }, cancellationToken);
    }
}
