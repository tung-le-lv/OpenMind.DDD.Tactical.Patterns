using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

public class ImportedItemsMustHaveValidPricesRule(IEnumerable<decimal> unitPrices) : IBusinessRule
{
    public bool IsBroken() => unitPrices.Any(price => price <= 0);

    public string Message => "All items must have a positive unit price.";

    public string Code => "INVALID_ITEM_PRICE";
}
