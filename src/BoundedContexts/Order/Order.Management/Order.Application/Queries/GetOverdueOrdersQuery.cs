using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

/// <summary>
/// Query to get orders that are overdue (submitted but not paid within threshold).
/// Uses the OverdueOrderSpecification from the Domain layer.
/// </summary>
public record GetOverdueOrdersQuery(int HoursThreshold = 24) : IRequest<IReadOnlyList<OrderDto>>;

public class GetOverdueOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOverdueOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetOverdueOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOverdueOrdersAsync(
            request.HoursThreshold,
            cancellationToken);

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
