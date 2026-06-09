using MediatR;
using Payment.Application.Services;
using Payment.Contracts;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

public record ProcessPaymentCommand(Guid PaymentId) : IRequest<bool>;

/// <summary>
/// Orchestrates a full payment attempt:
///   1. Load customer info via ICustomerInfoProvider (Customer-Supplier contract)
///   2. Validate domain rules via IPaymentProcessingService
///   3. Transition to Processing and persist
///   4. Call the payment gateway
///   5. Complete or fail the payment based on the gateway result
/// </summary>
public class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentProcessingService processingService,
    ICustomerInfoProvider customerInfoProvider,
    IPaymentGateway paymentGateway) : IRequestHandler<ProcessPaymentCommand, bool>
{
    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(request.PaymentId), cancellationToken);
        if (payment is null)
        {
            return false;
        }

        var customerInfo = await customerInfoProvider.GetCustomerInfoAsync(payment.CustomerId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Customer {payment.CustomerId.Value} not found");

        if (string.IsNullOrWhiteSpace(customerInfo.Email))
        {
            throw new InvalidOperationException($"Customer {payment.CustomerId.Value} has no email address");
        }

        if (string.IsNullOrWhiteSpace(customerInfo.BillingAddress.Street))
        {
            throw new InvalidOperationException($"Customer {payment.CustomerId.Value} has no billing address");
        }

        // Domain validation
        var validation = processingService.ValidatePayment(payment);
        if (!validation.IsValid)
        {
            payment.Fail(validation.ErrorMessage!);
            paymentRepository.Update(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return false;
        }

        // Transition to Processing and persist before calling the gateway so the
        // state is durable even if the process crashes mid-flight.
        payment.StartProcessing();
        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // Call the payment gateway
        var gatewayRequest = new GatewayChargeRequest(
            CustomerEmail:   customerInfo.Email,
            BillingAddress:  customerInfo.BillingAddress,
            Amount:          payment.Amount.Amount,
            Currency:        payment.Amount.Currency,
            CardLast4Digits: payment.CardDetails?.Last4Digits,
            CardType:        payment.CardDetails?.CardType,
            CardHolderName:  payment.CardDetails?.CardHolderName);

        var gatewayResult = await paymentGateway.ChargeAsync(gatewayRequest, cancellationToken);

        if (gatewayResult.Succeeded)
        {
            payment.Complete(gatewayResult.TransactionId!);
        }
        else
        {
            payment.Fail(gatewayResult.ErrorMessage!);
        }

        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return gatewayResult.Succeeded;
    }
}
