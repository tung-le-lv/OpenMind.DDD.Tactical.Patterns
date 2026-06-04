using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

public record GetPendingOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;

public class GetPendingOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetPendingOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetPendingOrdersAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
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
