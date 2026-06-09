using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get failed payments that need retry or investigation.
/// Uses the FailedPaymentSpecification from the Domain layer.
/// </summary>
public record GetFailedPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetFailedPaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetFailedPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetFailedPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetFailedPaymentsAsync(cancellationToken);

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
