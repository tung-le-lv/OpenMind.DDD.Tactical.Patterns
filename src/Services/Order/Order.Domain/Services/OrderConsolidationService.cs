using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;

namespace Order.Domain.Services;

/// DDD Domain Service:
/// 1. Contains domain logic that doesn't naturally fit in an Entity or Value Object
/// 2. Operates on multiple aggregates or external data
/// 3. Stateless
/// 4. Named using Ubiquitous Language
/// 
/// From Eric Evans DDD book:
/// Many domain or application SERVICES are built on top of populations of ENTITIES and VALUE OBJECTS, 
/// behaving like scripts that organize the domain’s potential to actually get things done. 
/// ENTITIES and VALUE OBJECTS are often too fine-grained to provide convenient access 
/// to the capabilities of the domain layer, which is where a very fine line between
/// the domain layer and the application layer appears.
/// For example, if a banking application can convert and export transactions into a spreadsheet file for analysis, 
/// that export is an application **SERVICE** because concepts like file formats have no meaning in the banking domain and involve no business rules. 
/// In contrast, a feature that transfers funds from one account to another is a domain **SERVICE** 
/// because it embeds significant business rules (such as crediting and debiting the appropriate accounts) 
/// and because “funds transfer” is a meaningful banking concept. In this case, the **SERVICE** itself does little work; 
/// instead, it asks the two **Account** objects to perform most of the behavior.
/// Placing the transfer operation on a single **Account** object would be awkward, 
/// however, because the operation involves two accounts and enforces global rules.

/// <summary>
/// Domain Service that consolidates two draft orders from the same customer into one.
///
/// This is a canonical DDD Domain Service: the operation spans two Order aggregates
/// and enforces global invariants that belong to neither aggregate alone.
/// Placing it on a single Order would be awkward — the operation involves two orders
/// and enforces cross-aggregate rules (same customer, same currency, combined item count).
/// The service itself does little work; it asks the two Order objects to do the heavy lifting.
///
/// This mirrors Evans' funds-transfer example exactly:
///   "The operation involves two accounts and enforces global rules. Placing the transfer
///    operation on a single Account object would be awkward."
/// </summary>
public interface IOrderConsolidationService : IDomainService
{
    /// <summary>
    /// Moves all items from <paramref name="sourceOrder"/> into <paramref name="targetOrder"/>,
    /// then cancels the source order.
    ///
    /// Preconditions enforced here (not in either aggregate, because neither owns the other):
    ///  - Both orders must belong to the same customer
    ///  - Both orders must be in Draft status
    ///  - Both orders must use the same currency
    ///  - The combined item count must not exceed the domain maximum
    /// </summary>
    void Consolidate(Aggregates.OrderAggregate.Order sourceOrder, Aggregates.OrderAggregate.Order targetOrder);
}

public class OrderConsolidationService : IOrderConsolidationService
{
    private const int MaxItemsPerOrder = 100;

    public void Consolidate(Aggregates.OrderAggregate.Order sourceOrder, Aggregates.OrderAggregate.Order targetOrder)
    {
        EnforceInvariants(sourceOrder, targetOrder);

        foreach (var item in sourceOrder.OrderItems)
        {
            targetOrder.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        sourceOrder.Cancel("Consolidated into order " + targetOrder.Id);
    }

    private static void EnforceInvariants(Aggregates.OrderAggregate.Order source, Aggregates.OrderAggregate.Order target)
    {
        if (source.CustomerId != target.CustomerId)
        {
            throw new DomainException("Cannot consolidate orders belonging to different customers.");
        }

        if (source.Status != OrderStatus.Draft || target.Status != OrderStatus.Draft)
        {
            throw new DomainException("Both orders must be in Draft status to consolidate.");
        }

        if (source.TotalAmount.Currency != target.TotalAmount.Currency)
        {
            throw new DomainException("Cannot consolidate orders with different currencies.");
        }

        var combinedItemCount = source.OrderItems.Count + target.OrderItems.Count;
        if (combinedItemCount > MaxItemsPerOrder)
        {
            throw new DomainException($"Consolidation would exceed the maximum of {MaxItemsPerOrder} items per order.");
        }
    }
}
