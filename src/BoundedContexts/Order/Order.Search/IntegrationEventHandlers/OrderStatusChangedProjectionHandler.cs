using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.IntegrationEvents;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.IntegrationEventHandlers;

public class OrderStatusChangedProjectionHandler(OrderSearchMongoDbContext context)
    : IIntegrationEventHandler<OrderStatusChangedIntegrationEvent>
{
    public async Task HandleAsync(OrderStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderReadModel>.Filter.Eq(o => o.Id, @event.OrderId);

        var update = Builders<OrderReadModel>.Update
            .Set(o => o.Status, @event.NewStatus)
            .Set(o => o.ModifiedAt, @event.ModifiedAt)
            .Set(o => o.Version, @event.Version);

        if (@event.SubmittedAt.HasValue)
        {
            update = update.Set(o => o.SubmittedAt, @event.SubmittedAt);
        }

        if (@event.PaidAt.HasValue)
        {
            update = update.Set(o => o.PaidAt, @event.PaidAt);
        }

        await context.Orders.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
