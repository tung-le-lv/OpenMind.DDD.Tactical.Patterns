using MediatR;
using Payment.Application.Commands;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentProcessingService processingService) : IRequestHandler<ProcessPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment == null)
        {
            return false;
        }

        var validationResult = processingService.ValidatePayment(payment);
        if (!validationResult.IsValid)
        {
            payment.Fail(validationResult.ErrorMessage!);
            paymentRepository.Update(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return false;
        }

        payment.StartProcessing();
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
