using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.DomainEventHandlers;

public class PaymentCreatedDomainEventHandler(ILogger<PaymentCreatedDomainEventHandler> logger) : INotificationHandler<PaymentCreatedDomainEvent>
{
    public Task Handle(PaymentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} created for Order {OrderId}, Amount: {Amount} {Currency}",
            notification.PaymentId.Value,
            notification.OrderId.Value,
            notification.Amount,
            notification.Currency);

        return Task.CompletedTask;
    }
}
