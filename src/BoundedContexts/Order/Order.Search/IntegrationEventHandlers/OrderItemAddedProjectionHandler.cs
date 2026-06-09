using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.Contracts.IntegrationEvents;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.IntegrationEventHandlers;

public class OrderItemAddedProjectionHandler(OrderSearchMongoDbContext context)
    : IIntegrationEventHandler<OrderItemAddedIntegrationEvent>
{
    public async Task HandleAsync(OrderItemAddedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderReadModel>.Filter.Eq(o => o.Id, @event.OrderId);

        if (@event.IsNewItem)
        {
            var newItem = new OrderItemReadModel
            {
                Id = @event.ItemId,
                ProductId = @event.ProductId,
                ProductName = @event.ProductName,
                UnitPriceAmount = @event.UnitPriceAmount,
                Currency = @event.Currency,
                Quantity = @event.Quantity,
                DiscountAmount = @event.DiscountAmount
            };

            var pushUpdate = Builders<OrderReadModel>.Update
                .Push(o => o.OrderItems, newItem)
                .Set(o => o.ModifiedAt, @event.ModifiedAt)
                .Set(o => o.Version, @event.Version);

            await context.Orders.UpdateOneAsync(filter, pushUpdate, cancellationToken: cancellationToken);
        }
        else
        {
            var itemFilter = Builders<OrderReadModel>.Filter.And(
                filter,
                Builders<OrderReadModel>.Filter.ElemMatch(o => o.OrderItems, i => i.Id == @event.ItemId));

            var setUpdate = Builders<OrderReadModel>.Update
                .Set("orderItems.$.quantity", @event.Quantity)
                .Set("orderItems.$.discountAmount", @event.DiscountAmount)
                .Set(o => o.ModifiedAt, @event.ModifiedAt)
                .Set(o => o.Version, @event.Version);

            await context.Orders.UpdateOneAsync(itemFilter, setUpdate, cancellationToken: cancellationToken);
        }
    }
}
