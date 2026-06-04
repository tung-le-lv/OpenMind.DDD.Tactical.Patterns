using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record SubmitOrderCommand(Guid OrderId) : IRequest<bool>;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<SubmitOrderCommand, bool>
{
    public async Task<bool> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        order.Submit();

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
