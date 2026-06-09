using BuildingBlocks.Integration;
using MediatR;
using Order.Application.Commands;
using Payment.Contracts.IntegrationEvents;

namespace Order.Application.IntegrationEventHandlers;

/// <summary>
/// Receives events from the Payment Bounded Context and translates them into commands
/// for the Order domain. This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class PaymentCompletedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<PaymentCompletedIntegrationEvent>
{
    public async Task HandleAsync(PaymentCompletedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new MarkOrderAsPaidCommand(@event.OrderId, @event.PaidAt), cancellationToken);
    }
}
