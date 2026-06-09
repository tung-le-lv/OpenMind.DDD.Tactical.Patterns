using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Card must not be expired for payment processing.
/// </summary>
public class CardMustNotBeExpiredRule(CardDetails? cardDetails) : IBusinessRule
{
    public bool IsBroken() => cardDetails != null && cardDetails.IsExpired();

    public string Message => "The card has expired. Please use a valid card.";
    
    public string Code => "CARD_EXPIRED";
}
