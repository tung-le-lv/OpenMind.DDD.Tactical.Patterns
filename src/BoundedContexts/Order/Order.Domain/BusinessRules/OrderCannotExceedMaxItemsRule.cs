using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order cannot exceed maximum items limit.
/// </summary>
public class OrderCannotExceedMaxItemsRule(int currentItemCount, int maxItems = 100) : IBusinessRule
{
    public bool IsBroken() => currentItemCount >= maxItems;

    public string Message => $"Order cannot have more than {maxItems} items. Current count: {currentItemCount}.";
    
    public string Code => "ORDER_MAX_ITEMS_EXCEEDED";
}
