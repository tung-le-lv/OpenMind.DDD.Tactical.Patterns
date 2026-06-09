using MediatR;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record RefundPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;

public class RefundPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<RefundPaymentCommand, bool>
{
    public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return false;
        }

        payment.Refund(request.Reason);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
