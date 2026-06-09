using BuildingBlocks.Domain;
using MediatR;
using Order.Application.DTOs;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

public record GetOrdersByStatusQuery(string Status) : IRequest<IReadOnlyList<OrderDto>>;

public class GetOrdersByStatusQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersByStatusQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var status = Enumeration.FromDisplayName<OrderStatus>(request.Status);
        var orders = await orderRepository.GetByStatusAsync(status, cancellationToken);
        return orders.Select(OrderDto.From).ToList();
    }
}
