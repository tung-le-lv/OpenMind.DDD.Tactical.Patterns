using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Entity representing an item within an Order.
///
/// Supple Design patterns applied:
/// - Standalone Class: depends only on Money and strongly-typed IDs — no specs or business rules
/// - Side-Effect-Free Functions (Evans): TotalForQuantity takes a quantity value and returns
///   a new Money result by composing Money's own side-effect-free operations — no field on
///   this entity is touched. Callers can invoke it speculatively (previewing a price before
///   committing a quantity change) because it is safe to combine freely with no risk of
///   unintended mutations. The immutable Money value object makes this guarantee structural.
/// - Closure of Operations: TotalForQuantity(int) → Money stays within the Money concept
/// - Assertions: ApplyDiscount states its postcondition explicitly
/// - Intention-Revealing Interfaces: TotalForQuantity and Total communicate domain queries clearly
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    /// Reference to the product in the Product Bounded Context — only the ID to keep contexts decoupled.
    public ProductId ProductId { get; private set; }

    /// Product name snapshot — protects the Order from Product context changes.
    public string ProductName { get; private set; }

    /// Unit price snapshot — protects from future price changes in the Product context.
    public Money UnitPrice { get; private set; }

    public int Quantity { get; private set; }
    public Money Discount { get; private set; }

    /// Side-Effect-Free: computed from current state, never mutates.
    public Money Total => UnitPrice.Multiply(Quantity) - Discount;

    private OrderItem() { }

    internal static OrderItem Create(
        ProductId productId,
        string productName,
        Money unitPrice,
        int quantity,
        Money? discount = null)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new OrderItem
        {
            Id = OrderItemId.New(),
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId)),
            ProductName = productName,
            UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice)),
            Quantity = quantity,
            Discount = discount ?? Money.Zero(unitPrice.Currency)
        };
    }

    // ── Side-Effect-Free Functions (Evans) ───────────────────────────────────
    // Each method below takes input values, computes a new Money result by composing
    // Money's own side-effect-free operations, and returns it — no field on
    // OrderItem is ever touched. Because they are purely computational they can be
    // called freely: in validation logic, pricing previews, or as part of a larger
    // calculation inside a command method, with no risk of unintended mutations.
    // The command methods below (SetQuantity, AddQuantity, ApplyDiscount) are
    // intentionally kept separate — they are the only place state changes happen.

    internal Money TotalForQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        return UnitPrice.Multiply(quantity) - Discount;
    }

    /// Computes the absolute discount amount that a percentage promotion would produce
    /// for this item, without touching any field. Used by Order.ApplyPromotion to
    /// calculate per-item discounts before committing any state changes.
    internal Money CalculateDiscountAmount(decimal percentage)
    {
        var gross = UnitPrice.Multiply(Quantity);
        return gross - gross.ApplyDiscount(percentage);
    }

    // ── Command methods (side-effecting, clearly separated from functions above) ─

    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        Quantity = quantity;
    }

    internal void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be positive", nameof(quantity));
        Quantity += quantity;
    }

    internal void ApplyDiscount(Money discount)
    {
        var itemTotal = UnitPrice.Multiply(Quantity);
        if (discount.Amount > itemTotal.Amount)
            throw new DomainException("Discount cannot exceed item total");

        Discount = discount;

        // Assertion: postcondition — Total must remain non-negative after discount is applied.
        System.Diagnostics.Debug.Assert(!Total.IsZero || discount == itemTotal,
            "Item total must be non-negative after discount.");
    }
}
