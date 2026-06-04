using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record MarkOrderAsPaidCommand(Guid OrderId, DateTime PaidAt) : IRequest<bool>;

public class MarkOrderAsPaidCommandHandler(IOrderRepository orderRepository) : IRequestHandler<MarkOrderAsPaidCommand, bool>
{
    public async Task<bool> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        order.MarkAsPaid(request.PaidAt);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
