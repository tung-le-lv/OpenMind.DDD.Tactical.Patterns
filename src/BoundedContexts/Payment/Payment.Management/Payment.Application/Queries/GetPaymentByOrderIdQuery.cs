using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentDto?>;

public class GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(OrderReference.From(request.OrderId), cancellationToken);
        return payment is null ? null : PaymentDto.From(payment);
    }
}
