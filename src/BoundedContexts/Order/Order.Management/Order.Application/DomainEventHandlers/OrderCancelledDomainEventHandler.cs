using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.DomainEventHandlers;

public class OrderCancelledDomainEventHandler(ILogger<OrderCancelledDomainEventHandler> logger) : INotificationHandler<OrderCancelledDomainEvent>
{
    public Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Order {OrderId} was cancelled. Reason: {Reason}",
            notification.OrderId.Value,
            notification.Reason);

        return Task.CompletedTask;
    }
}
