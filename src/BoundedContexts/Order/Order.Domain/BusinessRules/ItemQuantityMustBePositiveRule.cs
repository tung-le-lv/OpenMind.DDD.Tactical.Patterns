using BuildingBlocks.Domain.BusinessRules;
using Order.Domain.ValueObjects;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Item quantity must be positive.
/// </summary>
public class ItemQuantityMustBePositiveRule(int quantity) : IBusinessRule
{
    public bool IsBroken() => quantity <= 0;

    public string Message => $"Item quantity must be greater than zero. Provided: {quantity}.";
    
    public string Code => "INVALID_ITEM_QUANTITY";
}
