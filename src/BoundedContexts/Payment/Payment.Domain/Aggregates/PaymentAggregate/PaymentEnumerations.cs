using BuildingBlocks.Domain;

namespace Payment.Domain.Aggregates.PaymentAggregate;

/// <summary>
/// Payment Status Enumeration.
/// </summary>
public class PaymentStatus : Enumeration
{
    /// <summary>Payment has been created but not processed</summary>
    public static PaymentStatus Pending = new(1, nameof(Pending));

    /// <summary>Payment is being processed</summary>
    public static PaymentStatus Processing = new(2, nameof(Processing));

    /// <summary>Payment was successful</summary>
    public static PaymentStatus Completed = new(3, nameof(Completed));

    /// <summary>Payment failed</summary>
    public static PaymentStatus Failed = new(4, nameof(Failed));

    /// <summary>Payment was refunded</summary>
    public static PaymentStatus Refunded = new(5, nameof(Refunded));

    /// <summary>Payment was cancelled</summary>
    public static PaymentStatus Cancelled = new(6, nameof(Cancelled));

    public PaymentStatus(int id, string name) : base(id, name) { }

    /// <summary>
    /// Business rule: Can the payment be processed?
    /// </summary>
    public bool CanBeProcessed()
    {
        return this == Pending;
    }

    /// <summary>
    /// Business rule: Can the payment be refunded?
    /// </summary>
    public bool CanBeRefunded()
    {
        return this == Completed;
    }

    /// <summary>
    /// Business rule: Can the payment be cancelled?
    /// </summary>
    public bool CanBeCancelled()
    {
        return this == Pending;
    }
}

/// <summary>
/// Payment Method Enumeration.
/// </summary>
public class PaymentMethod : Enumeration
{
    public static PaymentMethod CreditCard = new(1, nameof(CreditCard));
    public static PaymentMethod DebitCard = new(2, nameof(DebitCard));
    public static PaymentMethod BankTransfer = new(3, nameof(BankTransfer));
    public static PaymentMethod PayPal = new(4, nameof(PayPal));
    public static PaymentMethod Cryptocurrency = new(5, nameof(Cryptocurrency));

    public PaymentMethod(int id, string name) : base(id, name) { }
}
