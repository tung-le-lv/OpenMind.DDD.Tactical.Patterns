using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order can only be modified when in Draft status.
/// </summary>
public class OrderMustBeInDraftStatusRule(Aggregates.OrderAggregate.OrderStatus currentStatus) : IBusinessRule
{
    public bool IsBroken() => !currentStatus.CanAddItems();

    public string Message => $"Order cannot be modified when in '{currentStatus.Name}' status. Only draft orders can be modified.";
    
    public string Code => "ORDER_NOT_MODIFIABLE";
}
