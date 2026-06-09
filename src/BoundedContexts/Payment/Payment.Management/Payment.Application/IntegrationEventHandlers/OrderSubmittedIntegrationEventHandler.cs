using BuildingBlocks.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Contracts.IntegrationEvents;
using Payment.Application.Commands;
using Payment.Application.DTOs;

namespace Payment.Application.IntegrationEventHandlers;

/// <summary>
/// Receives events from the Order Bounded Context and creates a payment request in the Payment domain.
/// This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class OrderSubmittedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderSubmittedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderSubmittedIntegrationEvent>
{
    public async Task HandleAsync(OrderSubmittedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling OrderSubmitted event for Order {OrderId}, Amount: {Amount} {Currency}",
            @event.OrderId,
            @event.TotalAmount,
            @event.Currency);

        var command = new CreatePaymentCommand(
            @event.OrderId,
            @event.CustomerId,
            @event.TotalAmount,
            @event.Currency,
            "CreditCard",
            new CardDetailsDto("4242", "Visa", 12, DateTime.UtcNow.Year + 2, "Demo Customer"));

        var paymentId = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} created for Order {OrderId}",
            paymentId,
            @event.OrderId);

        await mediator.Send(new ProcessPaymentCommand(paymentId), cancellationToken);

        await mediator.Send(new CompletePaymentCommand(paymentId, $"TXN-{Guid.NewGuid():N}"), cancellationToken);

        logger.LogInformation("Payment {PaymentId} processed and completed", paymentId);
    }
}
