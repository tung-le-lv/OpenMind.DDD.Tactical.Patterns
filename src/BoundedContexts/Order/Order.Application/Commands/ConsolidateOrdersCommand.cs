using MediatR;
using Order.Domain.Repositories;
using Order.Domain.Services;
using Order.Domain.ValueObjects;

namespace Order.Application.Commands;

public record ConsolidateOrdersCommand(Guid SourceOrderId, Guid TargetOrderId) : IRequest<bool>;

/// <summary>
/// Orchestrates the consolidation use case: load both aggregates, delegate to the
/// domain service, then persist both changes within a single Unit of Work.
///
/// The application handler owns no business logic — all invariants live in
/// OrderConsolidationService and the Order aggregate itself.
/// </summary>
public class ConsolidateOrdersCommandHandler(
    IOrderRepository orderRepository,
    IOrderConsolidationService consolidationService) : IRequestHandler<ConsolidateOrdersCommand, bool>
{
    public async Task<bool> Handle(ConsolidateOrdersCommand request, CancellationToken cancellationToken)
    {
        var sourceOrder = await orderRepository.GetByIdAsync(OrderId.From(request.SourceOrderId), cancellationToken);
        if (sourceOrder is null)
        {
            return false;
        }

        var targetOrder = await orderRepository.GetByIdAsync(OrderId.From(request.TargetOrderId), cancellationToken);
        if (targetOrder is null)
        {
            return false;
        }

        consolidationService.Consolidate(sourceOrder, targetOrder);

        orderRepository.Update(sourceOrder);
        orderRepository.Update(targetOrder);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
