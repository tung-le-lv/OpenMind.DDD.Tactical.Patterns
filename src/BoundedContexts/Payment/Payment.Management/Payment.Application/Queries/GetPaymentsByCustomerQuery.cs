using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Queries;

public record GetPaymentsByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<PaymentDto>>;

public class GetPaymentsByCustomerQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentsByCustomerQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetByCustomerIdAsync(CustomerReference.From(request.CustomerId), cancellationToken);
        return payments.Select(PaymentDto.From).ToList();
    }
}
