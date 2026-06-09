using MediatR;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record FailPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;

public class FailPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<FailPaymentCommand, bool>
{
    public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return false;
        }

        payment.Fail(request.Reason);
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
