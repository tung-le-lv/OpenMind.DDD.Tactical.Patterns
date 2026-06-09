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
        return payment is null ? null : PaymentDto.From(payment);
    }
}
