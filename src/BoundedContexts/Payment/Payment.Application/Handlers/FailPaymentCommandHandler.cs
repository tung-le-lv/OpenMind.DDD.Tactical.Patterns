using MediatR;
using Payment.Application.Commands;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class FailPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<FailPaymentCommand, bool>
{
    public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment == null)
        {
            return false;
        }

        payment.Fail(request.Reason);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
