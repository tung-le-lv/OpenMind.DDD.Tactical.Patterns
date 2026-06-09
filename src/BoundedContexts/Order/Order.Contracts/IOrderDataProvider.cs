namespace Order.Contracts;

/// <summary>
/// Customer-Supplier contract: Payment (customer) defines what Order data it needs.
/// Order (supplier) implements this interface.
///
/// This interface lives in Order.Contracts because it is part of the published language
/// that Order exposes. Payment drives the shape of the contract — it owns the definition
/// of what it needs — but Order owns the delivery.
/// </summary>
public interface IOrderDataProvider
{
    /// <summary>
    /// Returns the order data required to process a payment, or null if the order does not exist.
    /// </summary>
    Task<OrderPaymentData?> GetOrderPaymentDataAsync(Guid orderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// The subset of Order data that the Payment context needs. Shape is driven by Payment (the customer).
/// </summary>
public record OrderPaymentData(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    string Status);
