using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.BusinessRules;

public class PaymentAmountMustBePositiveRule(Money amount) : IBusinessRule
{
    public bool IsBroken() => !amount.IsChargeable();

    public string Message =>
        $"Payment amount must be at least {Money.MinimumChargeableAmount:C}. Provided: {amount}.";

    public string Code => "INVALID_PAYMENT_AMOUNT";
}
