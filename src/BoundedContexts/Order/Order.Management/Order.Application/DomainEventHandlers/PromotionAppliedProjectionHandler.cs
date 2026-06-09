using BuildingBlocks.Integration;
using MediatR;
using Order.Domain.Events;
using Order.IntegrationEvents;

namespace Order.Application.DomainEventHandlers;

public class PromotionAppliedProjectionHandler(IEventBus eventBus) : INotificationHandler<PromotionAppliedDomainEvent>
{
    public async Task Handle(PromotionAppliedDomainEvent notification, CancellationToken cancellationToken)
    {
        var itemDiscounts = notification.ItemDiscounts
            .Select(i => new OrderItemDiscountSnapshot(i.ItemId, i.DiscountAmount))
            .ToList();

        await eventBus.PublishAsync(new OrderPromotionAppliedIntegrationEvent
        {
            OrderId = notification.OrderId.Value,
            ItemDiscounts = itemDiscounts,
            ModifiedAt = notification.OccurredOn,
            Version = notification.Version
        }, cancellationToken);
    }
}
