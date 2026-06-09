namespace Payment.Contracts;

/// <summary>
/// Customer-Supplier contract for customer information.
///
/// Pattern:  Customer-Supplier  (Evans DDD, Chapter 14)
/// Customer: Payment BC  (downstream — needs the data to process payments)
/// Supplier: Order BC    (upstream — owns the Customer aggregate)
///
///   This models the real-world dynamic: the downstream team negotiates with the
///   upstream team and the upstream team commits to satisfying the downstream's needs.
///   Ownership of the interface by the customer makes that obligation explicit in code.
/// </summary>
public interface ICustomerInfoProvider
{
    /// <summary>
    /// Returns the customer information required by the Payment context to process a
    /// payment, or null if the customer does not exist.
    /// </summary>
    Task<CustomerInfo?> GetCustomerInfoAsync(Guid customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// The subset of Customer data that Payment needs.
/// Shape is driven entirely by Payment's requirements (billing address for the payment
/// gateway, email for the payment receipt).
/// </summary>
public record CustomerInfo(
    Guid CustomerId,
    string FullName,
    string Email,
    BillingAddress BillingAddress);

public record BillingAddress(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
