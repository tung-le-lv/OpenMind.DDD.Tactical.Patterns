using BuildingBlocks.Domain;
using Payment.Domain.Specifications;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Services;

/// <summary>
/// Domain Service for Payment Processing.
///
/// This domain service handles payment processing logic that doesn't
/// naturally belong to the Payment aggregate itself.
/// </summary>
public interface IPaymentProcessingService : IDomainService
{
    PaymentValidationResult ValidatePayment(Aggregates.PaymentAggregate.Payment payment);
    Money CalculateProcessingFee(Money amount, Aggregates.PaymentAggregate.PaymentMethod method);

    /// <summary>
    /// Checks if a payment requires additional verification based on value.
    /// Uses HighValuePaymentSpecification.
    /// </summary>
    bool RequiresAdditionalVerification(Aggregates.PaymentAggregate.Payment payment, decimal threshold = 1000m);
}

public record PaymentValidationResult(bool IsValid, string? ErrorMessage)
{
    public static PaymentValidationResult Success() => new(true, null);
    public static PaymentValidationResult Failure(string message) => new(false, message);
}

public class PaymentProcessingService : IPaymentProcessingService
{
    public PaymentValidationResult ValidatePayment(Aggregates.PaymentAggregate.Payment payment)
    {
        if (payment.Amount.Amount <= 0)
        {
            return PaymentValidationResult.Failure("Payment amount must be positive");
        }

        if (payment.CardDetails != null && payment.CardDetails.IsExpired())
        {
            return PaymentValidationResult.Failure("Card has expired");
        }

        var pendingSpec = new PendingPaymentSpecification();
        if (!pendingSpec.IsSatisfiedBy(payment))
        {
            return PaymentValidationResult.Failure($"Payment cannot be processed in {payment.Status.Name} status");
        }

        return PaymentValidationResult.Success();
    }

    public Money CalculateProcessingFee(Money amount, Aggregates.PaymentAggregate.PaymentMethod method)
    {
        decimal feePercentage = method.Name switch
        {
            nameof(Aggregates.PaymentAggregate.PaymentMethod.CreditCard) => 0.029m,
            nameof(Aggregates.PaymentAggregate.PaymentMethod.DebitCard) => 0.015m,
            nameof(Aggregates.PaymentAggregate.PaymentMethod.PayPal) => 0.034m,
            nameof(Aggregates.PaymentAggregate.PaymentMethod.BankTransfer) => 0.005m,
            _ => 0.03m
        };

        var fee = amount.Amount * feePercentage;
        return new Money(Math.Round(fee, 2), amount.Currency);
    }

    public bool RequiresAdditionalVerification(Aggregates.PaymentAggregate.Payment payment, decimal threshold = 1000m)
    {
        var highValueSpec = new HighValuePaymentSpecification(threshold);
        return highValueSpec.IsSatisfiedBy(payment);
    }
}
