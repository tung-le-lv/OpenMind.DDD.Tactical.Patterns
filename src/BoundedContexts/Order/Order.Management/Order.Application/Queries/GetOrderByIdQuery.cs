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
        return order is null ? null : OrderDto.From(order);
    }
}
