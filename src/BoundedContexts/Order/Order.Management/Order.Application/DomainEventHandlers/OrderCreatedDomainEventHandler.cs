using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandler(IEventBus eventBus) : INotificationHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            CustomerId = notification.CustomerId.Value,
            Street = notification.ShippingAddress.Street,
            City = notification.ShippingAddress.City,
            State = notification.ShippingAddress.State,
            Country = notification.ShippingAddress.Country,
            ZipCode = notification.ShippingAddress.ZipCode,
            Status = "Draft",
            Currency = notification.Currency,
            CreatedAt = notification.CreatedAt,
            Version = 0
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
