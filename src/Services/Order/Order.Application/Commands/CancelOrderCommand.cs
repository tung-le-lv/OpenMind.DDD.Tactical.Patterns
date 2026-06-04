using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<bool>;

public class CancelOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        order.Cancel(request.Reason);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
