using MediatR;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record CancelPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;

public class CancelPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<CancelPaymentCommand, bool>
{
    public async Task<bool> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return false;
        }

        payment.Cancel(request.Reason);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
