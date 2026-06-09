using BuildingBlocks.Domain;

namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object for PaymentId.
/// </summary>
public class PaymentId : ValueObject
{
    public Guid Value { get; }

    private PaymentId() { }

    public PaymentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static PaymentId New() => new(Guid.NewGuid());
    public static PaymentId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PaymentId id) => id.Value;
    public static implicit operator PaymentId(Guid guid) => new(guid);
}

/// <summary>
/// Value Object for OrderId reference from Order Bounded Context.
/// This maintains loose coupling between contexts.
/// </summary>
public class OrderReference : ValueObject
{
    public Guid Value { get; }

    private OrderReference() { }

    public OrderReference(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OrderReference cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static OrderReference From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderReference id) => id.Value;
    public static implicit operator OrderReference(Guid guid) => new(guid);
}

/// <summary>
/// Value Object for CustomerId reference.
/// </summary>
public class CustomerReference : ValueObject
{
    public Guid Value { get; }

    private CustomerReference() { }

    public CustomerReference(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CustomerReference cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static CustomerReference From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerReference id) => id.Value;
    public static implicit operator CustomerReference(Guid guid) => new(guid);
}
