using MediatR;
using Payment.Application.Commands;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class RefundPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<RefundPaymentCommand, bool>
{
    public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment == null)
        {
            return false;
        }

        payment.Refund(request.Reason);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
