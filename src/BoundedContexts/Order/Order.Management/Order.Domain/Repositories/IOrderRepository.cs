using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Domain.Repositories;

/// <summary>
/// Repository for Order Aggregate Root.
/// 
/// DDD Repository Pattern:
/// 1. One repository per Aggregate Root
/// 2. Provides collection-like interface
/// 3. Abstracts persistence mechanism
/// 4. Works only with Aggregate Roots
/// </summary>
public interface IOrderRepository : IRepository<Aggregates.OrderAggregate.Order, OrderId>
{
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds orders matching the given specification.
    /// Specifications can be composed using And, Or, Not operators.
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> FindAsync(
        Specification<Aggregates.OrderAggregate.Order> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders that are overdue (submitted but not paid within the specified hours).
    /// Uses OverdueOrderSpecification internally.
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetOverdueOrdersAsync(
        int hoursThreshold = 24,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders that can be cancelled.
    /// Uses CancellableOrderSpecification internally.
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetCancellableOrdersAsync(
        CancellationToken cancellationToken = default);
}
