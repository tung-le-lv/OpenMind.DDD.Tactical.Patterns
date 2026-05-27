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

    /// <summary>
    /// Read-only access to order items - external code cannot modify the collection directly.
    /// </summary>
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Money TotalAmount => CalculateTotalAmount();
    public string Currency { get; private set; }

    private Order() 
    { 
        _orderItems = new List<OrderItem>();
    }

    #region Factory Methods

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
        // Enforce business rules using IBusinessRule pattern
        CheckRule(new OrderMustBeInDraftStatusRule(Status));
        CheckRule(new ItemQuantityMustBePositiveRule(quantity));

        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            // Validate max items count before adding new item using IBusinessRule
            CheckRule(new OrderCannotExceedMaxItemsRule(_orderItems.Count));

            var newItem = OrderItem.Create(productId, productName, unitPrice, quantity);
            _orderItems.Add(newItem);
        }

        SetModified();

        Emit(new OrderItemAddedDomainEvent(Id, productId, productName, quantity));
    }

    public void RemoveItem(OrderItemId itemId)
    {
        // Use IBusinessRule for clear validation message
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
            throw new DomainException($"Order item {itemId} not found");

        _orderItems.Remove(item);
        SetModified();
    }

    public void UpdateItemQuantity(OrderItemId itemId, int newQuantity)
    {
        // Use IBusinessRule for clear validation message
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
            throw new DomainException($"Order item {itemId} not found");

        if (newQuantity <= 0)
        {
            // Validate min items count before removing
            var minItemsSpec = new MinItemsCountSpecification(1);
            var maxItemsSpec = new MaxItemsCountSpecification(100);
            var itemCountSpec = minItemsSpec & maxItemsSpec;
            
            _orderItems.Remove(item);
            
            if (!itemCountSpec.IsSatisfiedBy(this))
            {
                _orderItems.Add(item); // Rollback
                throw new DomainException("Order must have at least 1 item");
            }
        }
        else
        {
            item.SetQuantity(newQuantity);
        }

        SetModified();
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        // Use IBusinessRule for clear validation message
        CheckRule(new OrderMustBeInDraftStatusRule(Status));

        ShippingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        SetModified();
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
        SetModified();
    }

    /// <summary>
    /// State transition that raises domain event - triggers integration event for Payment Bounded Context.
    /// </summary>
    public void Submit(decimal minimumOrderValue = 10.00m)
    {
        if (!Status.CanBeSubmitted())
            throw new DomainException($"Cannot submit an order in {Status.Name} status");

        // Use IBusinessRule for validation with clear error message
        CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));

        var minimumValueSpec = new MinimumOrderValueSpecification(minimumOrderValue);
        if (!minimumValueSpec.IsSatisfiedBy(this))
            throw new DomainException($"Order total must be at least {minimumOrderValue:C}");

        Status = OrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        SetModified();

        // Raise domain event - this will trigger integration event for Payment service
        Emit(new OrderSubmittedDomainEvent(
            Id,
            CustomerId,
            TotalAmount.Amount,
            Currency));
    }

    /// <summary>
    /// Called when payment is confirmed from the Payment Bounded Context.
    /// </summary>
    public void MarkAsPaid(DateTime paidAt)
    {
        if (!Status.CanBePaid())
            throw new DomainException($"Cannot mark order as paid in {Status.Name} status");

        Status = OrderStatus.Paid;
        PaidAt = paidAt;
        SetModified();

        Emit(new OrderPaidDomainEvent(Id, paidAt));
    }

    public void MarkPaymentFailed(string reason)
    {
        if (!Status.CanMarkPaymentFailed())
            throw new DomainException($"Cannot mark payment as failed for order in {Status.Name} status");

        Status = OrderStatus.PaymentFailed;
        SetModified();

        Emit(new OrderPaymentFailedDomainEvent(Id, reason));
    }

    public void StartProcessing()
    {
        var readySpec = new OrderReadyForProcessingSpecification();
        if (!readySpec.IsSatisfiedBy(this))
            throw new DomainException("Only paid orders can be processed");

        Status = OrderStatus.Processing;
        SetModified();
    }

    public void Ship()
    {
        if (!Status.CanBeShipped())
            throw new DomainException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        SetModified();

        Emit(new OrderShippedDomainEvent(Id));
    }

    public void MarkAsDelivered()
    {
        if (!Status.CanBeDelivered())
            throw new DomainException("Only shipped orders can be marked as delivered");

        Status = OrderStatus.Delivered;
        SetModified();

        Emit(new OrderDeliveredDomainEvent(Id));
    }

    public void Cancel(string reason)
    {
        // Use IBusinessRule for validation with clear error message
        // Note: The CancellableOrderSpecification is still available for querying cancellable orders
        CheckRule(new OrderCannotBeCancelledAfterShippingRule(Status));

        Status = OrderStatus.Cancelled;
        SetModified();

        Emit(new OrderCancelledDomainEvent(Id, reason));
    }

    #endregion

    #region Private Methods

    private Money CalculateTotalAmount()
    {
        if (!_orderItems.Any())
            return Money.Zero(Currency);

        return _orderItems
            .Select(x => x.Total)
            .Aggregate((current, next) => current + next);
    }

    private void SetModified()
    {
        ModifiedAt = DateTime.UtcNow;
        IncrementVersion();
    }

    #endregion
}
