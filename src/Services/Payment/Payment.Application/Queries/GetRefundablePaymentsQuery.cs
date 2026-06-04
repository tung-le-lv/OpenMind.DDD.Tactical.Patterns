using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get payments that can be refunded.
/// Uses the RefundablePaymentSpecification from the Domain layer.
/// </summary>
public record GetRefundablePaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetRefundablePaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetRefundablePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetRefundablePaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetRefundablePaymentsAsync(cancellationToken);

        return payments.Select(MapToDto).ToList();
    }

    private static PaymentDto MapToDto(Domain.Aggregates.PaymentAggregate.Payment payment) => new(
        payment.Id.Value,
        payment.OrderId.Value,
        payment.CustomerId.Value,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status.Name,
        payment.Method.Name,
        payment.CardDetails != null ? new CardDetailsDto(
            payment.CardDetails.Last4Digits,
            payment.CardDetails.CardType,
            payment.CardDetails.ExpiryMonth,
            payment.CardDetails.ExpiryYear,
            payment.CardDetails.CardHolderName) : null,
        payment.TransactionId,
        payment.FailureReason,
        payment.CreatedAt,
        payment.ProcessedAt,
        payment.CompletedAt);
}
