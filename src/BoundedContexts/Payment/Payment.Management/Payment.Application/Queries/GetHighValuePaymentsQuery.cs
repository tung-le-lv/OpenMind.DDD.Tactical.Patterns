using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get high-value payments that may require additional verification.
/// Uses the HighValuePaymentSpecification from the Domain layer.
/// </summary>
public record GetHighValuePaymentsQuery(decimal Threshold = 1000m) : IRequest<IReadOnlyList<PaymentDto>>;

public class GetHighValuePaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetHighValuePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetHighValuePaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetHighValuePaymentsAsync(request.Threshold, cancellationToken);
        return payments.Select(PaymentDto.From).ToList();
    }
}
