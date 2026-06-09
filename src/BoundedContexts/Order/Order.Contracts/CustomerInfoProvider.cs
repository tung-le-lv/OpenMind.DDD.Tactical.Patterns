using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Payment.Contracts;

namespace Order.Contracts;

/// <summary>
/// Supplier-side implementation of the Customer-Supplier contract.
///
/// Lives in Order.Contracts — the package Order publishes to consumers — so that
/// any downstream context can wire up ICustomerInfoProvider by referencing only
/// Order.Contracts, without pulling in Order's full application layer.
///
/// Order adapts its Customer aggregate to the shape Payment declared it needs.
/// Order does not decide what fields are returned; Payment already did via
/// ICustomerInfoProvider in Payment.Contracts.
/// </summary>
public class CustomerInfoProvider(ICustomerRepository customerRepository) : ICustomerInfoProvider
{
    public async Task<CustomerInfo?> GetCustomerInfoAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(CustomerId.From(customerId), cancellationToken);
        if (customer is null)
        {
            return null;
        }

        return new CustomerInfo(
            customer.Id.Value,
            customer.FullName,
            customer.Email.Value,
            new BillingAddress(
                customer.BillingAddress.Street,
                customer.BillingAddress.City,
                customer.BillingAddress.State,
                customer.BillingAddress.Country,
                customer.BillingAddress.ZipCode));
    }
}
