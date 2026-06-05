# Domain-Driven Design

An implementation of Domain-Driven Design Tactical Patterns.

## Table of Contents

- [References](#references)
- [Tactical Design Patterns](#tactical-design-patterns)
  - [Entities](#entities)
  - [Value Objects](#value-objects)
  - [Aggregates](#aggregates)
  - [Domain Events](#domain-events)
  - [Repository Pattern](#repository-pattern)
  - [Domain Services](#domain-services)
  - [Specification Pattern](#specification-pattern)
  - [Factory Pattern](#factory-pattern)
  - [Enumeration Pattern](#enumeration-pattern)
- [Supple Design Patterns](#supple-design-patterns)
  - [Intention-Revealing Interfaces](#intention-revealing-interfaces)
  - [Side-Effect-Free Functions](#side-effect-free-functions)
  - [Assertions](#assertions)
  - [Conceptual Contours](#conceptual-contours)
  - [Standalone Class](#standalone-class)
  - [Closure of Operations](#closure-of-operations)
- [Integration Between Bounded Contexts](#integration-between-bounded-contexts-context-mapping)

## References

- Evans, Eric. "Domain-Driven Design: Tackling Complexity in the Heart of Software"
- Vernon, Vaughn. "Domain-Driven Design Distilled"
- Vernon, Vaughn. "Implementing Domain-Driven Design"

## Tactical Design Patterns

### Entities
Objects defined by their identity, not their attributes.
```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; }
    // Identity-based equality
}
```

### Value Objects
Immutable objects defined by their attributes.
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    // Attribute-based equality
}
```

### Aggregates
Consistency boundaries with a single entry point.
```csharp
public class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    // Only Order can modify OrderItems
    public void AddItem(ProductId productId, ...) { ... }
}
```

### Domain Events
Facts about what happened in the domain.
```csharp
public record OrderSubmittedDomainEvent : DomainEventBase
{
    public OrderId OrderId { get; }
    public decimal TotalAmount { get; }
}
```

### Repository Pattern
Collection-like interface for aggregates.
```csharp
public interface IOrderRepository : IRepository<Order, OrderId>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId);
}
```

### Domain Services
1. Contains domain logic that doesn't naturally fit in an Entity or Value Object
2. Operates on multiple aggregates or external data
3. Stateless
4. Named using Ubiquitous Language

```csharp
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
```

### Specification Pattern
Encapsulated business rules for **querying and filtering**.
```csharp
public class OrderReadyForProcessingSpecification : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression()
        => order => order.Status == OrderStatus.Paid;
}

// Usage: Filtering/Querying
var overdueOrders = await repository.FindAsync(new OverdueOrderSpecification(24));
var cancellableOrders = await repository.FindAsync(new CancellableOrderSpecification());

// Composable with And, Or, Not
var spec = new MinimumOrderValueSpecification(100) & new CancellableOrderSpecification();
```

### Factory Pattern
Encapsulated object creation.
```csharp
public static Order Create(CustomerId customerId, Address address)
{
    var order = new Order { ... };
    order.RaiseDomainEvent(new OrderCreatedDomainEvent(...));
    return order;
}
```

### Enumeration Pattern
Type-safe, behavior-rich enumerations.
```csharp
public class OrderStatus : Enumeration
{
    public static OrderStatus Draft = new(1, nameof(Draft));
    public static OrderStatus Submitted = new(2, nameof(Submitted));
    
    public bool CanBeCancelled() => this == Draft || this == Submitted;
}
```

## Supple Design Patterns

Supple Design is a set of complementary patterns from Evans' Blue Book (Ch. 10) that make a domain model easier to work with safely. Where tactical patterns answer *what* to build, Supple Design answers *how* to shape the code so the model stays expressive and resistant to corruption over time.

---

### Intention-Revealing Interfaces

**What:** Name every method and type so the *why* is obvious from the signature alone — no reader should need to look inside to understand what a call means in domain terms.

**Why it matters:** When names reveal intent, callers can reason about consequences without studying implementations, and mistakes become obvious at the call site rather than buried in a stack trace.

**In this codebase:**

`OrderStatus` exposes domain predicates instead of raw comparisons:

```csharp
// OrderStatus.cs
public bool CanBeCancelled() => this == Draft || this == Submitted || this == PaymentFailed;
public bool CanBeSubmitted()  => Equals(this, Draft);
public bool CanBePaid()       => Equals(this, Submitted);
public bool CanBeShipped()    => Equals(this, Processing);
```

`Order` exposes named eligibility predicates that aggregate multiple conditions:

```csharp
// Order.cs
public bool IsEligibleForSubmission(decimal minimumOrderValue = 10.00m)
    => Status.CanBeSubmitted()
       && _orderItems.Count > 0
       && TotalAmount.IsGreaterThanOrEqualTo(Money.FromDecimal(minimumOrderValue, Currency));

public bool IsEligibleForCancellation() => Status.CanBeCancelled();
```

`Money` expresses comparisons in domain vocabulary rather than arithmetic:

```csharp
// Order.Domain/ValueObjects/Money.cs
public bool IsZero => Amount == 0;

public bool IsGreaterThanOrEqualTo(Money threshold)
{
    EnsureSameCurrency(threshold);
    return Amount >= threshold.Amount;
}
```

---

### Side-Effect-Free Functions

**What:** Separate computation from mutation. Pure functions take inputs, return a result, and change *nothing*. Commands mutate state but delegate all non-trivial calculations to pure functions first.

**Why it matters:** Pure functions can be called anywhere — in validation, in tests, speculatively — without fear of accidentally changing state. When computation and mutation are mixed, every call becomes a risk.

**In this codebase:**

`Order.ApplyPromotion` runs all computation before touching any state. If validation fails, the order is completely unchanged — there is no partial state to roll back:

```csharp
// Order.cs
public void ApplyPromotion(Percentage discount, decimal minimumOrderValueAfterDiscount = 0m)
{
    // Phase 1 — pure computation, nothing mutated yet
    var projectedTotal  = TotalAmount.ApplyDiscount(discount);
    var discountsPerItem = _orderItems
        .Select(item => (item, discount: item.CalculateDiscountAmount(discount)))
        .ToList();

    // Phase 2 — validate computed results, still no mutation
    CheckRule(new PromotionTotalMustMeetMinimumRule(projectedTotal, minimumOrderValueAfterDiscount, Currency));

    // Phase 3 — mutations only after every check passes
    foreach (var (item, discountAmount) in discountsPerItem)
        item.ApplyDiscount(discountAmount);

    SetModified();
    Emit(new PromotionAppliedDomainEvent(...));
}
```

`OrderItem` exposes pure helpers that callers use before deciding whether to commit:

```csharp
// OrderItem.cs
// Returns the line total for an arbitrary quantity — no field is touched.
internal Money TotalForQuantity(int quantity) => UnitPrice.Multiply(quantity) - Discount;

// Computes what the discount would be — used by Order.ApplyPromotion in Phase 1.
internal Money CalculateDiscountAmount(Percentage discount)
{
    var gross = UnitPrice.Multiply(Quantity);
    return gross - gross.ApplyDiscount(discount);
}
```

`Money` arithmetic methods all return new instances — the value object is structurally immutable:

```csharp
// Order.Domain/ValueObjects/Money.cs
public Money Add(Money other)      => new(Amount + other.Amount, Currency);
public Money Subtract(Money other) => new(Amount - other.Amount, Currency);
public Money Multiply(int n)       => new(Amount * n, Currency);
public Money ApplyDiscount(Percentage d) => new(Amount - d.ApplyTo(Amount), Currency);
```

---

### Assertions

**What:** State postconditions explicitly after every mutation so that the system's guarantees are visible in the code, not just in the developer's head. Use `Debug.Assert` for invariants that *must* hold immediately after a state change — they document what the next reader can count on.

**Why it matters:** Without stated postconditions, callers must infer what a method guarantees by reading its entire body. Assertions make those guarantees machine-checked documentation. They also catch logic errors during development before they silently corrupt downstream state.

**In this codebase:**

Every state-transition method in `Order` asserts its own postconditions:

```csharp
// Order.cs — Submit
Status     = OrderStatus.Submitted;
SubmittedAt = DateTime.UtcNow;
// ...
Debug.Assert(SubmittedAt.HasValue,             "SubmittedAt must be set after Submit.");
Debug.Assert(Status == OrderStatus.Submitted,  "Status must be Submitted after Submit.");

// Order.cs — Cancel
Status = OrderStatus.Cancelled;
// ...
Debug.Assert(Equals(Status, OrderStatus.Cancelled),   "Status must be Cancelled after Cancel.");
Debug.Assert(!IsEligibleForCancellation(),             "A cancelled order must not be eligible for cancellation again.");

// Order.cs — UpdateItemQuantity
Debug.Assert(_orderItems.Count >= 1, "Order must have at least one item after quantity update.");

// Order.cs — ApplyPromotion
Debug.Assert(
    TotalAmount.IsGreaterThanOrEqualTo(Money.Zero(Currency)),
    "Order total must remain non-negative after promotion.");
```

`OrderItem` asserts its own discount postcondition:

```csharp
// OrderItem.cs — ApplyDiscount
Discount = discount;
Debug.Assert(!Total.IsZero || discount == itemTotal,
    "Item total must be non-negative after discount.");
```

---

### Conceptual Contours

**What:** Decompose logic at the natural seams of the domain, not at arbitrary technical boundaries. When a concept in the domain has a clear name and meaning, it deserves its own method or type — even if the implementation is a single line.

**Why it matters:** Splitting logic at the wrong place forces callers to reconstruct intent by combining pieces. Splitting at the right place means a caller can read `Submit` and immediately understand the precondition as a domain concept, not as an assembly of raw checks.

**In this codebase:**

`Order.Submit` delegates its precondition check to `IsEligibleForSubmission`, carving the validation boundary along a natural domain concept rather than inlining the conditions directly:

```csharp
// Order.cs
public void Submit(decimal minimumOrderValue = 10.00m)
{
    // Conceptual Contours: the eligibility concept is named and isolated.
    // Submit is only responsible for the transition, not for defining what "eligible" means.
    if (!IsEligibleForSubmission(minimumOrderValue))
    {
        if (!Status.CanBeSubmitted())
            throw new DomainException($"Cannot submit an order in {Status.Name} status");
        CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));
        throw new DomainException($"Order total must be at least {minimumOrderValue:C}");
    }

    Status      = OrderStatus.Submitted;
    SubmittedAt = DateTime.UtcNow;
    // ...
}
```

`OrderConsolidationService` is itself a Conceptual Contour — the cross-aggregate consistency rules for merging two orders live in a named domain service rather than leaking into either aggregate:

```csharp
// OrderConsolidationService.cs
public void Consolidate(Order sourceOrder, Order targetOrder)
{
    // Cross-aggregate invariants — neither aggregate owns the other,
    // so the contour belongs here.
    EnforceInvariants(sourceOrder, targetOrder);

    foreach (var item in sourceOrder.OrderItems)
        targetOrder.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);

    sourceOrder.Cancel("Consolidated into order " + targetOrder.Id);
}
```

---

### Standalone Class

**What:** A class is standalone when it can be fully understood and tested in isolation — it imports nothing from the broader domain model, only primitive types or base classes from a shared kernel.

**Why it matters:** Every dependency a class carries is knowledge a reader must hold in their head simultaneously. A standalone class has zero cognitive dependencies on the rest of the domain — it can be reasoned about, tested, and reused without pulling in anything else.

**In this codebase:**

`Percentage` documents the pattern explicitly and depends on nothing from the Order domain:

```csharp
// Order.Domain/ValueObjects/Percentage.cs
// Standalone Class (Evans ch.10):
// This Percentage class imports nothing from the Order domain —
// only ValueObject from BuildingBlocks and .NET primitives.
// It can be understood, tested, and reused without loading any other domain concept.
public class Percentage : ValueObject
{
    public decimal Value { get; }
    // ...
}
```

`Money` is similarly standalone — its only external reference is `Percentage`, which is itself standalone:

```csharp
// Order.Domain/ValueObjects/Money.cs
// Standalone Class: depends only on ValueObject and Percentage —
// both standalone value objects with no aggregate or service dependencies.
public class Money : ValueObject
{
    public Money Add(Money other)            => new(Amount + other.Amount, Currency);
    public Money ApplyDiscount(Percentage d) => new(Amount - d.ApplyTo(Amount), Currency);
}
```

`OrderItem` is standalone within the aggregate — it depends only on `Money` and strongly-typed IDs, with no references to specifications or domain services:

```csharp
// OrderItem.cs
// Standalone Class: depends only on Money and strongly-typed IDs —
// no specs or business rules, nothing that would require loading external concepts.
public class OrderItem : Entity<OrderItemId>
{
    public Money Total => UnitPrice.Multiply(Quantity) - Discount;
}
```

---

### Closure of Operations

**What:** Design operations so that their return type is the same as the types of their arguments — the operation stays "inside" the concept. This allows unlimited chaining without ever crossing a type boundary.

**Why it matters:** When operations are closed, callers can compose complex calculations as fluent chains (`price.ApplyDiscount(10%).Add(tax)`) without needing intermediate variables of different types. It also signals that the type is complete — it carries all the operations relevant to its concept.

**In this codebase:**

`Money` arithmetic is closed — every method takes `Money` (or a scalar) and returns `Money`, so callers never leave the concept:

```csharp
// Order.Domain/ValueObjects/Money.cs
// Closure of Operations: every arithmetic operation returns Money.
// Callers can compose chains like: price.ApplyDiscount(discount).Add(tax)
// without ever leaving the Money concept.
public Money Add(Money other)            => new(Amount + other.Amount, Currency);
public Money Subtract(Money other)       => new(Amount - other.Amount, Currency);
public Money Multiply(int multiplier)    => new(Amount * multiplier, Currency);
public Money ApplyDiscount(Percentage d) => new(Amount - d.ApplyTo(Amount), Currency);

public static Money operator +(Money left, Money right) => left.Add(right);
public static Money operator -(Money left, Money right) => left.Subtract(right);
```

`Percentage` is closed under all its operations:

```csharp
// Order.Domain/ValueObjects/Percentage.cs
// Closure of Operations: every method returns Percentage,
// keeping all composition within the type.
public Percentage Add(Percentage other)      => new(Math.Min(100m, Value + other.Value));
public Percentage Subtract(Percentage other) => new(Math.Max(0m, Value - other.Value));
public Percentage Complement()               => new(100m - Value);
public Percentage Cap(Percentage ceiling)    => new(Math.Min(Value, ceiling.Value));
```

`OrderItem.TotalForQuantity` demonstrates closure between `OrderItem` operations and `Money` — the result stays in the `Money` concept rather than escaping to a primitive:

```csharp
// OrderItem.cs
// Closure of Operations: TotalForQuantity(int) → Money stays within the Money concept.
public Money Total                          => UnitPrice.Multiply(Quantity) - Discount;
internal Money TotalForQuantity(int quantity) => UnitPrice.Multiply(quantity) - Discount;
```

---

## Integration Between Bounded Contexts (Context Mapping)

### Published Language + Anti-Corruption Layer

1. **Order Submitted**: Order service raises `OrderSubmittedDomainEvent`
2. **Domain Event Handler**: Converts to `OrderSubmittedIntegrationEvent`
3. **Event Bus**: Publishes integration event
4. **Payment Handler**: Creates and processes payment
5. **Payment Completed**: Payment service raises `PaymentCompletedDomainEvent`
6. **Integration Event**: `PaymentCompletedIntegrationEvent` published
7. **Order Handler**: Updates order status to Paid

```
┌─────────────────┐                    ┌─────────────────┐
│  Order Service  │                    │ Payment Service │
├─────────────────┤                    ├─────────────────┤
│ Order.Submit()  │                    │                 │
│       │         │                    │                 │
│       ▼         │                    │                 │
│ OrderSubmitted  │ ──Integration──►   │ Create Payment  │
│ DomainEvent     │    Event           │       │         │
│                 │                    │       ▼         │
│                 │                    │ Process Payment │
│                 │                    │       │         │
│                 │                    │       ▼         │
│ MarkAsPaid()    │ ◄──Integration──   │ PaymentCompleted│
│       │         │    Event           │ DomainEvent     │
│       ▼         │                    │                 │
│ Status = Paid   │                    │                 │
└─────────────────┘                    └─────────────────┘
```

