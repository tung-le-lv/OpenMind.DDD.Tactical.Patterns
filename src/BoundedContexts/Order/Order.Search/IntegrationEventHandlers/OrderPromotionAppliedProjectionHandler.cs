using BuildingBlocks.Integration;
using MongoDB.Bson;
using MongoDB.Driver;
using Order.IntegrationEvents;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.IntegrationEventHandlers;

public class OrderPromotionAppliedProjectionHandler(OrderSearchMongoDbContext context)
    : IIntegrationEventHandler<OrderPromotionAppliedIntegrationEvent>
{
    public async Task HandleAsync(OrderPromotionAppliedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderReadModel>.Filter.Eq(o => o.Id, @event.OrderId);

        // Build a pipeline update to set each item's discount using arrayFilters
        var writes = new List<WriteModel<OrderReadModel>>();

        foreach (var item in @event.ItemDiscounts)
        {
            var itemFilter = Builders<OrderReadModel>.Filter.And(
                filter,
                Builders<OrderReadModel>.Filter.ElemMatch(o => o.OrderItems, i => i.Id == item.ItemId));

            var update = Builders<OrderReadModel>.Update
                .Set("orderItems.$.discountAmount", item.DiscountAmount);

            writes.Add(new UpdateOneModel<OrderReadModel>(itemFilter, update));
        }

        if (writes.Count > 0)
        {
            await context.Orders.BulkWriteAsync(writes, cancellationToken: cancellationToken);
        }

        // Update ModifiedAt and Version on the parent document
        var metaUpdate = Builders<OrderReadModel>.Update
            .Set(o => o.ModifiedAt, @event.ModifiedAt)
            .Set(o => o.Version, @event.Version);

        await context.Orders.UpdateOneAsync(filter, metaUpdate, cancellationToken: cancellationToken);
    }
}
