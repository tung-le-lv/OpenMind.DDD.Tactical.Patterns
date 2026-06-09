using Order.Domain.Aggregates.CustomerAggregate;
using Order.Domain.ValueObjects;

namespace Order.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
}
