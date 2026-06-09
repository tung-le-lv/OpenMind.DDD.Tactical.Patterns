using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Queries;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<OrderDto>>;

public class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(CustomerId.From(request.CustomerId), cancellationToken);
        return orders.Select(OrderDto.From).ToList();
    }
}
