using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    private static OrderDto MapToDto(Domain.Aggregates.OrderAggregate.Order order) => new(
        order.Id.Value,
        order.CustomerId.Value,
        order.Status.Name,
        order.TotalAmount.Amount,
        order.Currency,
        new AddressDto(
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.Country,
            order.ShippingAddress.ZipCode),
        order.OrderItems.Select(item => new OrderItemDto(
            item.Id.Value,
            item.ProductId.Value,
            item.ProductName,
            item.UnitPrice.Amount,
            item.Quantity,
            item.Discount.Amount,
            item.Total.Amount)).ToList(),
        order.Notes,
        order.CreatedAt,
        order.SubmittedAt,
        order.PaidAt);
}
