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
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetRefundablePaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetRefundablePaymentsAsync(cancellationToken);
        return payments.Select(PaymentDto.From).ToList();
    }
}
