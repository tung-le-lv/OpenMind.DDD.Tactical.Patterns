using BuildingBlocks.Domain;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates.CustomerAggregate;

/// <summary>
/// Customer Aggregate Root — lives inside the Order Bounded Context for simplicity.
/// In a production system this would be its own Bounded Context (Customer BC).
///
/// The Customer aggregate owns the authoritative email address and billing address.
/// The Payment BC needs this data to route receipts and to satisfy payment-gateway
/// requirements. Rather than Payment reaching into Order's internals, the two teams
/// agreed on a contract — see <see cref="Payment.Contracts.ICustomerInfoProvider"/>.
/// </summary>
public class Customer : AggregateRoot<CustomerId>
{
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public Address BillingAddress { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private Customer() { }

    /// <summary>
    /// Rehydrates a Customer from persistence. No domain events emitted.
    /// </summary>
    public static Customer Reconstitute(
        CustomerId id,
        string fullName,
        Email email,
        Address billingAddress,
        DateTime registeredAt,
        int version)
    {
        return new Customer
        {
            Id             = id,
            FullName       = fullName,
            Email          = email,
            BillingAddress = billingAddress,
            RegisteredAt   = registeredAt,
            Version        = version
        };
    }

    /// <summary>
    /// Registers a new customer. Invariants are enforced at creation time.
    /// </summary>
    public static Customer Register(string fullName, Email email, Address billingAddress)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required", nameof(fullName));
        }

        return new Customer
        {
            Id             = CustomerId.New(),
            FullName       = fullName,
            Email          = email  ?? throw new ArgumentNullException(nameof(email)),
            BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress)),
            RegisteredAt   = DateTime.UtcNow
        };
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        IncrementVersion();
    }

    public void UpdateBillingAddress(Address newAddress)
    {
        BillingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        IncrementVersion();
    }
}
