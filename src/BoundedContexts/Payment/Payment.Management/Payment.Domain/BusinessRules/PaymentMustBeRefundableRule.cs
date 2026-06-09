using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Payment can only be refunded when in completed status.
/// </summary>
public class PaymentMustBeRefundableRule(PaymentStatus currentStatus) : IBusinessRule
{
    public bool IsBroken() => !currentStatus.CanBeRefunded();

    public string Message => $"Payment cannot be refunded when in '{currentStatus.Name}' status. Only completed payments can be refunded.";
    
    public string Code => "PAYMENT_NOT_REFUNDABLE";
}
