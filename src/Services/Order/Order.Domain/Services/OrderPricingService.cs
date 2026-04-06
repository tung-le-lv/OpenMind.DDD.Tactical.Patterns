using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Specifications;
using Order.Domain.ValueObjects;

namespace Order.Domain.Services;

/// <summary>
/// Domain Service for Order pricing calculations.
/// 
/// DDD Domain Service:
/// 1. Contains domain logic that doesn't naturally fit in an Entity or Value Object
/// 2. Operates on multiple aggregates or external data
/// 3. Stateless
/// 4. Named using Ubiquitous Language
/// 
/// From Eric Evans DDD book:
/// Many domain or application **SERVICES** are built on top of populations of **ENTITIES** and **VALUE OBJECTS**, 
/// behaving like scripts that organize the domain’s potential to actually get things done. 
/// **ENTITIES** and **VALUE OBJECTS** are often too fine-grained to provide convenient access 
/// to the capabilities of the domain layer, which is where a very fine line between
/// the domain layer and the application layer appears.
/// For example, if a banking application can convert and export transactions into a spreadsheet file for analysis, 
/// that export is an application **SERVICE** because concepts like file formats have no meaning in the banking domain and involve no business rules. 
/// In contrast, a feature that transfers funds from one account to another is a domain **SERVICE** 
/// because it embeds significant business rules (such as crediting and debiting the appropriate accounts) 
/// and because “funds transfer” is a meaningful banking concept. In this case, the **SERVICE** itself does little work; 
/// instead, it asks the two **Account** objects to perform most of the behavior.
///  Placing the transfer operation on a single **Account** object would be awkward, 
/// however, because the operation involves two accounts and enforces global rules.
/// </summary>
public interface IOrderPricingService : IDomainService
{
    /// <summary>
    /// Calculates the total price including discounts and taxes.
    /// </summary>
    Money CalculateFinalPrice(Aggregates.OrderAggregate.Order order, decimal discountPercentage = 0);

    /// <summary>
    /// Validates if a discount code is applicable to the order.
    /// Uses MinimumOrderValueSpecification to ensure minimum order value.
    /// </summary>
    bool IsDiscountApplicable(Aggregates.OrderAggregate.Order order, string discountCode);

    /// <summary>
    /// Checks if an order qualifies for free shipping.
    /// Uses MinimumOrderValueSpecification with a threshold.
    /// </summary>
    bool QualifiesForFreeShipping(Aggregates.OrderAggregate.Order order, decimal freeShippingThreshold = 100m);
}

public class OrderPricingService : IOrderPricingService
{
    public Money CalculateFinalPrice(Aggregates.OrderAggregate.Order order, decimal discountPercentage = 0)
    {
        var subtotal = order.TotalAmount;

        if (discountPercentage is > 0 and <= 100)
        {
            var discountAmount = subtotal.Amount * (discountPercentage / 100);
            return new Money(subtotal.Amount - discountAmount, subtotal.Currency);
        }

        return subtotal;
    }

    public bool IsDiscountApplicable(Aggregates.OrderAggregate.Order order, string discountCode)
    {
        if (string.IsNullOrEmpty(discountCode))
            return false;

        // Use Specification pattern to check minimum order value for discount eligibility
        var minimumValueSpec = new MinimumOrderValueSpecification(50m);
        var draftStatusSpec = order.Status == OrderStatus.Draft;
        
        return draftStatusSpec && minimumValueSpec.IsSatisfiedBy(order);
    }

    public bool QualifiesForFreeShipping(Aggregates.OrderAggregate.Order order, decimal freeShippingThreshold = 100m)
    {
        // Use Specification pattern to check if order meets free shipping threshold
        var freeShippingSpec = new MinimumOrderValueSpecification(freeShippingThreshold);
        return freeShippingSpec.IsSatisfiedBy(order);
    }
}
