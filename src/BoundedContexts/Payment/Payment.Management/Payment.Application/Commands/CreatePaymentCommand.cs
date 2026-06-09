using BuildingBlocks.Domain;
using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record CreatePaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency = "USD",
    string Method = "CreditCard",
    CardDetailsDto? CardDetails = null) : IRequest<Guid>;

public class CreatePaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var method = Enumeration.FromDisplayName<PaymentMethod>(request.Method);

        CardDetails? cardDetails = null;
        if (request.CardDetails != null)
        {
            cardDetails = new CardDetails(
                request.CardDetails.Last4Digits,
                request.CardDetails.CardType,
                request.CardDetails.ExpiryMonth,
                request.CardDetails.ExpiryYear,
                request.CardDetails.CardHolderName);
        }

        var payment = Payment.Domain.Aggregates.PaymentAggregate.Payment.CreateForOrder(
            OrderReference.From(request.OrderId),
            CustomerReference.From(request.CustomerId),
            new Money(request.Amount, request.Currency),
            method,
            cardDetails);

        await paymentRepository.AddAsync(payment, cancellationToken);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return payment.Id.Value;
    }
}
