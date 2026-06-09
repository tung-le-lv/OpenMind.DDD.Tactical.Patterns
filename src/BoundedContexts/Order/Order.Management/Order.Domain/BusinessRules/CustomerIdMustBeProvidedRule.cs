using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

public class CustomerIdMustBeProvidedRule(Guid customerId) : IBusinessRule
{
    public bool IsBroken() => customerId == Guid.Empty;

    public string Message => "Customer ID is required to create an order.";

    public string Code => "CUSTOMER_ID_REQUIRED";
}
