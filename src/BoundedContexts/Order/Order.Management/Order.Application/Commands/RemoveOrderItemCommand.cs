using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record RemoveOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<bool>;

public class RemoveOrderItemCommandHandler(IOrderRepository orderRepository) : IRequestHandler<RemoveOrderItemCommand, bool>
{
    public async Task<bool> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
        {
            return false;
        }

        order.RemoveItem(OrderItemId.From(request.ItemId));

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
