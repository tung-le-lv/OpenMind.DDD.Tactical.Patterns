using Order.Contracts;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Services;

/// <summary>
/// Supplier-side implementation of the Customer-Supplier contract.
/// Order fulfills what Payment declared it needs via IOrderDataProvider.
/// </summary>
public class OrderDataProvider(IOrderRepository orderRepository) : IOrderDataProvider
{
    public async Task<OrderPaymentData?> GetOrderPaymentDataAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(orderId), cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderPaymentData(
            order.Id.Value,
            order.CustomerId.Value,
            order.TotalAmount.Amount,
            order.Currency,
            order.Status.Name);
    }
}
