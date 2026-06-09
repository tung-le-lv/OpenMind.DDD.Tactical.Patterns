using BuildingBlocks.Domain;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Order Status using the Enumeration pattern from DDD.
/// This provides type-safe, behavior-rich status values.
/// Popular library: https://github.com/ardalis/SmartEnum
/// </summary>
public class OrderStatus(int id, string name) : Enumeration(id, name)
{
    public static OrderStatus Draft = new(1, nameof(Draft));
    public static OrderStatus Submitted = new(2, nameof(Submitted));
    public static OrderStatus Paid = new(3, nameof(Paid));
    public static OrderStatus Processing = new(4, nameof(Processing));
    public static OrderStatus Shipped = new(5, nameof(Shipped));
    public static OrderStatus Delivered = new(6, nameof(Delivered));
    public static OrderStatus Cancelled = new(7, nameof(Cancelled));
    public static OrderStatus PaymentFailed = new(8, nameof(PaymentFailed));

    public bool CanBeCancelled()
    {
        return this == Draft || this == Submitted || this == PaymentFailed;
    }

    public bool CanAddItems()
    {
        return Equals(this, Draft);
    }

    public bool CanBeSubmitted()
    {
        return Equals(this, Draft);
    }

    public bool CanBePaid()
    {
        return Equals(this, Submitted);
    }

    public bool CanMarkPaymentFailed()
    {
        return Equals(this, Submitted);
    }

    public bool CanBeShipped()
    {
        return Equals(this, Processing);
    }

    public bool CanBeDelivered()
    {
        return Equals(this, Shipped);
    }
}
