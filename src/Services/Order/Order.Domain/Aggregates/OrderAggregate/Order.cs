using System.Diagnostics;
using BuildingBlocks.Domain;
using Order.Domain.BusinessRules;
using Order.Domain.Events;
using Order.Domain.Specifications;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Order Aggregate Root.
///
/// DDD Principles applied:
/// 1. Aggregate Root: Order is the single entry point for the Order aggregate
/// 2. Consistency Boundary: All invariants within the aggregate are enforced here
/// 3. Encapsulation: Internal entities (OrderItems) cannot be modified directly
/// 4. Domain Events: Business events are raised for important state changes
/// 5. Rich Domain Model: Business logic lives in the domain, not in services

/// Supple Design patterns applied:
/// - Intention-Revealing Interfaces: ItemCount, IsEligibleForSubmission, IsEligibleForCancellation
///   expose domain concepts rather than raw internals
/// - Side-Effect-Free Functions (Evans): see ApplyPromotion for the full pattern.
///   The command first calls ProjectTotalWithDiscount and item.CalculateDiscountAmount
///   (pure computations — no state touched) to validate the projected outcome, then
///   applies all mutations only after every check passes. Evans' core idea: extract
///   complex calculations into side-effect-free functions so commands are only
///   responsible for mutating state, not for mixing computation with mutation.
/// - Assertions: every state transition states its postcondition with Debug.Assert
///   so readers know what the system guarantees after each command
/// - Conceptual Contours: Submit delegates its eligibility check to IsEligibleForSubmission,
///   carving the validation boundary cleanly along the domain concept
/// </summary>
public class Order : AggregateRoot<OrderId>
{
    private List<OrderItem> _orderItems;

    public CustomerId CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? Notes { get; private set; }
    
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Money TotalAmount => CalculateTotalAmount();
    public string Currency { get; private set; }

    // ── Intention-Revealing Interfaces (query predicates) ────────────────────
    // These express domain concepts as named predicates rather than raw state
    // comparisons. They read state but do not compute or return new values —
    // they are NOT Evans' Side-Effect-Free Functions; they are CQS-style queries.

    /// Returns true when the order can legally be submitted at the given minimum value.
    public bool IsEligibleForSubmission(decimal minimumOrderValue = 10.00m)
        => Status.CanBeSubmitted()
           && _orderItems.Count > 0
           && TotalAmount.IsGreaterThanOrEqualTo(Money.FromDecimal(minimumOrderValue, Currency));

    /// Returns true when the order can legally be cancelled at this point in its lifecycle.
    public bool IsEligibleForCancellation() => Status.CanBeCancelled();
    
    #region Factory Methods

    /// Rehydrates an Order from a persistence document. No domain events are emitted.
    public static Order Reconstitute(
        OrderId id,
        CustomerId customerId,
        Address shippingAddress,
        OrderStatus status,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? submittedAt,
        DateTime? paidAt,
        string? notes,
        string currency,
        int version,
        IEnumerable<OrderItem> orderItems)
    {
        return new Order
        {
            Id              = id,
            CustomerId      = customerId,
            ShippingAddress = shippingAddress,
            Status          = status,
            CreatedAt       = createdAt,
            ModifiedAt      = modifiedAt,
            SubmittedAt     = submittedAt,
            PaidAt          = paidAt,
            Notes           = notes,
            Currency        = currency,
            Version         = version,
            _orderItems = [.. orderItems]
        };
    }

    /// <summary>
    /// Factory method ensures all invariants are met at creation time.
    /// </summary>
    public static Order Create(CustomerId customerId, Address shippingAddress, string currency = "USD")
    {
        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId)),
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress)),
            Status = OrderStatus.Draft,
            Currency = currency,
            CreatedAt = DateTime.UtcNow
        };

        order.Emit(new OrderCreatedDomainEvent(order.Id, order.CustomerId));

        return order;
    }

    #endregion

    #region Behavior Methods - Rich Domain Model
    
    public void AddItem(ProductId productId, string productName, Money unitPrice, int quantity)
    {
        CheckRule(new OrderMustBeInDraftStatusRule(Status));
        CheckRule(new ItemQuantityMustBePositiveRule(quantity));

        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            CheckRule(new OrderCannotExceedMaxItemsRule(_orderItems.Count));

            var newItem = OrderItem.Create(productId, productName, unitPrice, quantity);
            _orderItems.Add(newItem);
        }

        SetModified();

        Emit(new OrderItemAddedDomainEvent(Id, productId, productName, quantity));
    }

    public void RemoveItem(OrderItemId itemId)
    {
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
        {
            throw new DomainException($"Order item {itemId} not found");
        }

        _orderItems.Remove(item);
        SetModified();
    }

    public void UpdateItemQuantity(OrderItemId itemId, int newQuantity)
    {
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId)
            ?? throw new DomainException($"Order item {itemId} not found");

        if (newQuantity <= 0)
        {
            // Assertion: check precondition before mutating — no rollback needed.
            if (_orderItems.Count <= 1)
            {
                throw new DomainException("Cannot remove the last item from an order.");
            }

            _orderItems.Remove(item);
        }
        else
        {
            item.SetQuantity(newQuantity);
        }

        SetModified();

        // Assertion: postcondition — order must always retain at least one item.
        Debug.Assert(_orderItems.Count >= 1, "Order must have at least one item after quantity update.");
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        ShippingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        SetModified();
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
        SetModified();
    }

    /// Applies a percentage promotion discount to all items in the order.
    ///
    /// Side-Effect-Free Functions pattern (Evans) in action:
    ///   Phase 1 — pure computation, nothing mutated yet:
    ///     - ProjectTotalWithDiscount computes the projected order total after discount
    ///     - item.CalculateDiscountAmount computes the absolute discount per item
    ///   Phase 2 — validation against the computed results (still no mutation):
    ///     - rejects if the projected total falls below the minimum order value
    ///   Phase 3 — state mutations, only after all checks pass:
    ///     - item.ApplyDiscount writes the computed discount onto each item
    ///     - SetModified and Emit record the transition
    ///
    /// If Phase 1-2 throws, the order is completely unchanged — no partial state to roll back.
    public void ApplyPromotion(Percentage discount, decimal minimumOrderValueAfterDiscount = 0m)
    {
        CheckRule(new OrderMustBeInDraftStatusRule(Status));
        CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));

        // Phase 1: Side effect free function. Pure computation — no state touched.
        // The method ApplyDiscount or CalculateDiscountAmount (containing algorithm) can be tested independently
        var projectedTotal = TotalAmount.ApplyDiscount(discount);
        var discountsPerItem = _orderItems
            .Select(item => (item, discount: item.CalculateDiscountAmount(discount)))
            .ToList();

        // Phase 2: validate the computed results — no mutation.
        CheckRule(new PromotionTotalMustMeetMinimumRule(projectedTotal, minimumOrderValueAfterDiscount, Currency));

        // Phase 3: Apply state change
        var originalTotal = TotalAmount;
        foreach (var (item, discountAmount) in discountsPerItem)
        {
            item.ApplyDiscount(discountAmount);
        }

        SetModified();
        Emit(new PromotionAppliedDomainEvent(Id, discount.Value, originalTotal.Amount, projectedTotal.Amount, Currency));

        Debug.Assert(
            TotalAmount.IsGreaterThanOrEqualTo(Money.Zero(Currency)),
            "Order total must remain non-negative after promotion.");
    }

    /// Transitions the order to Submitted status and triggers payment.
    /// Conceptual Contours: delegates eligibility check to IsEligibleForSubmission,
    /// carving the validation boundary along the natural domain concept.
    public void Submit(decimal minimumOrderValue = 10.00m)
    {
        // Use the side-effect-free query to check readiness before mutating.
        if (!IsEligibleForSubmission(minimumOrderValue))
        {
            if (!Status.CanBeSubmitted())
            {
                throw new DomainException($"Cannot submit an order in {Status.Name} status");
            }

            CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));
            throw new DomainException($"Order total must be at least {minimumOrderValue:C}");
        }

        Status = OrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        SetModified();

        Emit(new OrderSubmittedDomainEvent(Id, CustomerId, TotalAmount.Amount, Currency));

        // Assertion: postcondition — submitted orders always have a timestamp.
        Debug.Assert(SubmittedAt.HasValue, "SubmittedAt must be set after Submit.");
        Debug.Assert(Status == OrderStatus.Submitted, "Status must be Submitted after Submit.");
    }

    /// Called when payment is confirmed from the Payment Bounded Context.
    public void MarkAsPaid(DateTime paidAt)
    {
        if (!Status.CanBePaid())
        {
            throw new DomainException($"Cannot mark order as paid in {Status.Name} status");
        }

        Status = OrderStatus.Paid;
        PaidAt = paidAt;
        SetModified();

        Emit(new OrderPaidDomainEvent(Id, paidAt));

        // Assertion: postcondition — a paid order always records when payment occurred.
        Debug.Assert(PaidAt.HasValue, "PaidAt must be set after MarkAsPaid.");
        Debug.Assert(Status == OrderStatus.Paid, "Status must be Paid after MarkAsPaid.");
    }

    public void MarkPaymentFailed(string reason)
    {
        if (!Status.CanMarkPaymentFailed())
        {
            throw new DomainException($"Cannot mark payment as failed for order in {Status.Name} status");
        }

        Status = OrderStatus.PaymentFailed;
        SetModified();

        Emit(new OrderPaymentFailedDomainEvent(Id, reason));

        Debug.Assert(Status == OrderStatus.PaymentFailed, "Status must be PaymentFailed after MarkPaymentFailed.");
    }

    public void StartProcessing()
    {
        var readySpec = new OrderReadyForProcessingSpecification();
        if (!readySpec.IsSatisfiedBy(this))
        {
            throw new DomainException("Only paid orders can be processed");
        }

        Status = OrderStatus.Processing;
        SetModified();

        Debug.Assert(Status == OrderStatus.Processing, "Status must be Processing after StartProcessing.");
    }

    public void Ship()
    {
        if (!Status.CanBeShipped())
        {
            throw new DomainException("Only processing orders can be shipped");
        }

        Status = OrderStatus.Shipped;
        SetModified();

        Emit(new OrderShippedDomainEvent(Id));

        Debug.Assert(Status == OrderStatus.Shipped, "Status must be Shipped after Ship.");
    }

    public void MarkAsDelivered()
    {
        if (!Status.CanBeDelivered())
        {
            throw new DomainException("Only shipped orders can be marked as delivered");
        }

        Status = OrderStatus.Delivered;
        SetModified();

        Emit(new OrderDeliveredDomainEvent(Id));

        Debug.Assert(Status == OrderStatus.Delivered, "Status must be Delivered after MarkAsDelivered.");
    }

    public void Cancel(string reason)
    {
        CheckRule(new OrderCannotBeCancelledAfterShippingRule(Status));

        Status = OrderStatus.Cancelled;
        SetModified();

        Emit(new OrderCancelledDomainEvent(Id, reason));

        // Assertion: postcondition — once cancelled, the order cannot be modified further.
        Debug.Assert(Equals(Status, OrderStatus.Cancelled), "Status must be Cancelled after Cancel.");
        Debug.Assert(!IsEligibleForCancellation(), "A cancelled order must not be eligible for cancellation again.");
    }

    #endregion
    
    private Money CalculateTotalAmount()
    {
        if (_orderItems.Count == 0)
        {
            return Money.Zero(Currency);
        }

        return _orderItems
            .Select(x => x.Total)
            .Aggregate((current, next) => current + next);
    }

    private void SetModified()
    {
        ModifiedAt = DateTime.UtcNow;
        IncrementVersion();
    }

}
