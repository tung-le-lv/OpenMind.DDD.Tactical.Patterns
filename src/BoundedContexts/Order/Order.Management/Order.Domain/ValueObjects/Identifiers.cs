using BuildingBlocks.Domain;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Strongly-typed ID for Order aggregate.
/// Using Value Objects for IDs provides type safety and avoids primitive obsession.
/// </summary>
public class OrderId : ValueObject
{
    public Guid Value { get; }

    private OrderId() { } // EF Core

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OrderId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderId orderId) => orderId.Value;
    public static implicit operator OrderId(Guid guid) => new(guid);
}

/// <summary>
/// Strongly-typed ID for OrderItem entity.
/// </summary>
public class OrderItemId : ValueObject
{
    public Guid Value { get; }

    private OrderItemId() { }

    public OrderItemId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OrderItemId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static OrderItemId New() => new(Guid.NewGuid());
    public static OrderItemId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderItemId id) => id.Value;
    public static implicit operator OrderItemId(Guid guid) => new(guid);
}

/// <summary>
/// Value Object for CustomerId - references to another Bounded Context.
/// </summary>
public class CustomerId : ValueObject
{
    public Guid Value { get; }

    private CustomerId() { }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static CustomerId New() => new(Guid.NewGuid());
    public static CustomerId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId id) => id.Value;
    public static implicit operator CustomerId(Guid guid) => new(guid);
}

/// <summary>
/// Value Object for ProductId - reference to Product Bounded Context.
/// </summary>
public class ProductId : ValueObject
{
    public Guid Value { get; }

    private ProductId() { }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static ProductId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ProductId id) => id.Value;
    public static implicit operator ProductId(Guid guid) => new(guid);
}
