using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

/// <summary>
/// Query to get orders that can be cancelled.
/// Uses the CancellableOrderSpecification from the Domain layer.
/// </summary>
public record GetCancellableOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;

public class GetCancellableOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetCancellableOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetCancellableOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetCancellableOrdersAsync(cancellationToken);
        return orders.Select(OrderDto.From).ToList();
    }
}
