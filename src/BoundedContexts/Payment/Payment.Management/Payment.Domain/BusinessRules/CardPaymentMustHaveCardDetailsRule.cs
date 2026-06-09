using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Card payments require card details.
/// </summary>
public class CardPaymentMustHaveCardDetailsRule(PaymentMethod method, bool hasCardDetails) : IBusinessRule
{
    public bool IsBroken()
    {
        var isCardPayment = method == PaymentMethod.CreditCard || method == PaymentMethod.DebitCard;
        return isCardPayment && !hasCardDetails;
    }

    public string Message => "Card details are required for credit card and debit card payments.";
    
    public string Code => "CARD_DETAILS_REQUIRED";
}
