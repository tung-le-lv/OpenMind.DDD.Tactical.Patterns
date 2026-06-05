using MediatR;
using Payment.Application.Commands;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class CompletePaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<CompletePaymentCommand, bool>
{
    public async Task<bool> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment == null)
        {
            return false;
        }

        payment.Complete(request.TransactionId);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
