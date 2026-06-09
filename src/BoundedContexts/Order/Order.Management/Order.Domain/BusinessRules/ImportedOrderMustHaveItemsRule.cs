using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

public class ImportedOrderMustHaveItemsRule(int itemCount) : IBusinessRule
{
    public bool IsBroken() => itemCount < 1;

    public string Message => "Imported order must contain at least one item.";

    public string Code => "IMPORT_ORDER_EMPTY";
}
