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
    public async Task<IReadOnlyList<OrderDto>> Handle(GetOverdueOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOverdueOrdersAsync(request.HoursThreshold, cancellationToken);
        return orders.Select(OrderDto.From).ToList();
    }
}
