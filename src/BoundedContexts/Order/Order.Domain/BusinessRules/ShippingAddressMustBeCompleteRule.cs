using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

public class ShippingAddressMustBeCompleteRule(
    string? street,
    string? city,
    string? country,
    string? zipCode) : IBusinessRule
{
    public bool IsBroken() =>
        string.IsNullOrWhiteSpace(street) ||
        string.IsNullOrWhiteSpace(city) ||
        string.IsNullOrWhiteSpace(country) ||
        string.IsNullOrWhiteSpace(zipCode);

    public string Message => "Shipping address must include street, city, country, and zip code.";

    public string Code => "INCOMPLETE_SHIPPING_ADDRESS";
}
