using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

/// <summary>
/// Converts the domain event to two integration events:
/// 1. OrderSubmittedIntegrationEvent — for the Payment Bounded Context (Anti-Corruption Layer pattern)
/// 2. OrderStatusChangedIntegrationEvent — for the Order Search projection
/// </summary>
public class OrderSubmittedDomainEventHandler(IEventBus eventBus) : INotificationHandler<OrderSubmittedDomainEvent>
{
    public async Task Handle(OrderSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderSubmittedIntegrationEvent(
            notification.OrderId.Value,
            notification.CustomerId.Value,
            notification.TotalAmount,
            notification.Currency), cancellationToken);

        await eventBus.PublishAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            NewStatus = "Submitted",
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version,
            SubmittedAt = notification.SubmittedAt
        }, cancellationToken);
    }
}
