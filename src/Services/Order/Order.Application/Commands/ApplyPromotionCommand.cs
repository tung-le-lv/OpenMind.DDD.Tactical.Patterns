using MediatR;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record ApplyPromotionCommand(Guid OrderId, decimal DiscountPercentage, decimal MinimumOrderValueAfterDiscount) : IRequest<bool>;

/// <summary>
/// Demonstrates how the application layer stays thin when domain logic uses
/// Side-Effect-Free Functions correctly. The handler does not calculate anything —
/// it loads the aggregate, delegates the entire computation-then-mutation sequence
/// to order.ApplyPromotion, and saves. All pricing logic lives in the domain.
/// </summary>
public class ApplyPromotionCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<ApplyPromotionCommand, bool>
{
    public async Task<bool> Handle(ApplyPromotionCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
        {
            return false;
        }

        // ApplyPromotion internally:
        //   1. Computes projected total and per-item discounts (side-effect-free)
        //   2. Validates the projected outcome
        //   3. Writes the computed discounts to each item (state mutation)
        // The handler knows nothing about how discounts are calculated.
        order.ApplyPromotion(Percentage.Of(request.DiscountPercentage), request.MinimumOrderValueAfterDiscount);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
