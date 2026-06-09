using MediatR;
using Payment.Domain.Events;

namespace Payment.Application.DomainEventHandlers;

public class PaymentCreatedDomainEventHandler : INotificationHandler<PaymentCreatedDomainEvent>
{
    public Task Handle(PaymentCreatedDomainEvent notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
