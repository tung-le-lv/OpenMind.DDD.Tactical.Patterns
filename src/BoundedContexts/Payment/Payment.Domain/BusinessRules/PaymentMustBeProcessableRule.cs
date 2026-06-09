using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Payment can only be processed when in a valid status.
/// </summary>
public class PaymentMustBeProcessableRule(PaymentStatus currentStatus) : IBusinessRule
{
    public bool IsBroken() => !currentStatus.CanBeProcessed();

    public string Message => $"Payment cannot be processed when in '{currentStatus.Name}' status.";
    
    public string Code => "PAYMENT_NOT_PROCESSABLE";
}
