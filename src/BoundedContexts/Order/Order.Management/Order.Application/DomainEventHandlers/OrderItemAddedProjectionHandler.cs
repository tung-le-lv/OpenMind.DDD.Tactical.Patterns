using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.Contracts.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderItemAddedProjectionHandler(IEventBus eventBus) : INotificationHandler<OrderItemAddedDomainEvent>
{
    public async Task Handle(OrderItemAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderItemAddedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            ItemId = notification.ItemId,
            ProductId = notification.ProductId.Value,
            ProductName = notification.ProductName,
            UnitPriceAmount = notification.UnitPriceAmount,
            Currency = notification.Currency,
            Quantity = notification.Quantity,
            DiscountAmount = notification.DiscountAmount,
            IsNewItem = notification.IsNewItem,
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
