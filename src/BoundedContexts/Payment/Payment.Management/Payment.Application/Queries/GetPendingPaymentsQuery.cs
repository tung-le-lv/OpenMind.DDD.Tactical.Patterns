using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

public record GetPendingPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetPendingPaymentsQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPendingPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPendingPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetPendingPaymentsAsync(cancellationToken);
        return payments.Select(PaymentDto.From).ToList();
    }
}
