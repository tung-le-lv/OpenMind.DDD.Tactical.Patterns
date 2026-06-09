using BuildingBlocks.Integration;
using MediatR;
using Order.Contracts.IntegrationEvents;
using Payment.Application.Commands;
using Payment.Application.DTOs;

namespace Payment.Application.IntegrationEventHandlers;

/// <summary>
/// Receives events from the Order Bounded Context and creates a payment request in the Payment domain.
/// This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class OrderSubmittedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderSubmittedIntegrationEvent>
{
    public async Task HandleAsync(OrderSubmittedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var command = new CreatePaymentCommand(
            @event.OrderId,
            @event.CustomerId,
            @event.TotalAmount,
            @event.Currency,
            "CreditCard",
            new CardDetailsDto("4242", "Visa", 12, DateTime.UtcNow.Year + 2, "Demo Customer"));

        var paymentId = await mediator.Send(command, cancellationToken);

        await mediator.Send(new ProcessPaymentCommand(paymentId), cancellationToken);
    }
}
