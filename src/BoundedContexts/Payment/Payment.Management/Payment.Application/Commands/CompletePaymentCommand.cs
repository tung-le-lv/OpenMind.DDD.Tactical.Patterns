using MediatR;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record CompletePaymentCommand(Guid PaymentId, string TransactionId) : IRequest<bool>;

public class CompletePaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<CompletePaymentCommand, bool>
{
    public async Task<bool> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return false;
        }

        payment.Complete(request.TransactionId);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
