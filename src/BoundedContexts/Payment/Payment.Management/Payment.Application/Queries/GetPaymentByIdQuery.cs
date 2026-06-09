using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Queries;

public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto?>;

public class GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return null;
        }

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
