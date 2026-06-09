using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order must have at least one item to be submitted.
/// </summary>
public class OrderMustHaveAtLeastOneItemRule(int itemCount) : IBusinessRule
{
    public bool IsBroken() => itemCount < 1;

    public string Message => "Order must have at least one item before it can be submitted.";
    
    public string Code => "ORDER_EMPTY";
}
