using BuildingBlocks.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Commands;
using Payment.IntegrationEvents;

namespace Order.Application.IntegrationEventHandlers;

/// <summary>
/// Receives events from the Payment Bounded Context and translates them into commands
/// for the Order domain. This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class PaymentFailedIntegrationEventHandler(
    IMediator mediator,
    ILogger<PaymentFailedIntegrationEventHandler> logger) : IIntegrationEventHandler<PaymentFailedIntegrationEvent>
{
    public async Task HandleAsync(PaymentFailedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Handling PaymentFailed event for Order {OrderId}, Reason: {Reason}",
            @event.OrderId,
            @event.Reason);

        await mediator.Send(new MarkOrderPaymentFailedCommand(@event.OrderId, @event.Reason), cancellationToken);

        logger.LogInformation("Order {OrderId} marked as payment failed", @event.OrderId);
    }
}
