using MediatR;
using Order.Contracts;
using Payment.Application.Commands;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

/// <summary>
/// Customer-Supplier: Payment (customer) calls IOrderDataProvider (supplied by Order)
/// to verify the order amount matches before processing the payment.
/// </summary>
public class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentProcessingService processingService,
    IOrderDataProvider orderDataProvider) : IRequestHandler<ProcessPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment == null)
        {
            return false;
        }

        var orderData = await orderDataProvider.GetOrderPaymentDataAsync(payment.OrderId.Value, cancellationToken);

        if (orderData is null)
        {
            payment.Fail("Order not found");
            paymentRepository.Update(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return false;
        }

        // Order must be in Submitted state — only submitted orders are payable.
        if (orderData.Status != "Submitted")
        {
            payment.Fail($"Cannot process payment: order is in '{orderData.Status}' status, expected 'Submitted'");
            paymentRepository.Update(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return false;
        }

        if (orderData.TotalAmount != payment.Amount.Amount || orderData.Currency != payment.Amount.Currency)
        {
            payment.Fail($"Payment amount {payment.Amount.Amount} {payment.Amount.Currency} does not match order total {orderData.TotalAmount} {orderData.Currency}");
            paymentRepository.Update(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
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
