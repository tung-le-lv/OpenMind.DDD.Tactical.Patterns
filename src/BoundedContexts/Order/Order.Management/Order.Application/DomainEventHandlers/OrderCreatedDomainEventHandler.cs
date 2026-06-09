using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger) : INotificationHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId}",
            notification.OrderId.Value,
            notification.CustomerId.Value);

        return Task.CompletedTask;
    }
}
