using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

/// <summary>
/// Converts the domain event to an integration event and publishes it through the event bus
/// for other bounded contexts. This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class OrderSubmittedDomainEventHandler(IEventBus eventBus) : INotificationHandler<OrderSubmittedDomainEvent>
{
    public async Task Handle(OrderSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderSubmittedIntegrationEvent(
            notification.OrderId.Value,
            notification.CustomerId.Value,
            notification.TotalAmount,
            notification.Currency);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
