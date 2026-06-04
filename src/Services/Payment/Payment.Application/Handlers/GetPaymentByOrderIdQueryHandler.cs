using MediatR;
using Payment.Application.DTOs;
using Payment.Application.Queries;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(
            OrderReference.From(request.OrderId),
            cancellationToken);

        if (payment == null)
            return null;

        return MapToDto(payment);
    }

    private static PaymentDto MapToDto(Domain.Aggregates.PaymentAggregate.Payment payment) => new(
        payment.Id.Value,
        payment.OrderId.Value,
        payment.CustomerId.Value,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status.Name,
        payment.Method.Name,
        payment.CardDetails != null
            ? new CardDetailsDto(
                payment.CardDetails.Last4Digits,
                payment.CardDetails.CardType,
                payment.CardDetails.ExpiryMonth,
                payment.CardDetails.ExpiryYear,
                payment.CardDetails.CardHolderName)
            : null,
        payment.TransactionId,
        payment.FailureReason,
        payment.CreatedAt,
        payment.ProcessedAt,
        payment.CompletedAt);
}
