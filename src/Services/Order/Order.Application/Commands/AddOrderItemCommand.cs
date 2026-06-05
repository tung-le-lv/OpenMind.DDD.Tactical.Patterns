using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record AddOrderItemCommand(Guid OrderId, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity) : IRequest<bool>;

public class AddOrderItemCommandHandler(IOrderRepository orderRepository) : IRequestHandler<AddOrderItemCommand, bool>
{
    public async Task<bool> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
        {
            return false;
        }

        order.AddItem(
            ProductId.From(request.ProductId),
            request.ProductName,
            Money.FromDecimal(request.UnitPrice),
            request.Quantity);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
