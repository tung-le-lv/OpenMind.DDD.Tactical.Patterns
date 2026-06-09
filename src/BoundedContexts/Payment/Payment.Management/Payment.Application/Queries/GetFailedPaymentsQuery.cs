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
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetFailedPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetFailedPaymentsAsync(cancellationToken);
        return payments.Select(PaymentDto.From).ToList();
    }
}
