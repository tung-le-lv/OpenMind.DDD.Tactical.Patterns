using BuildingBlocks.Integration;
using Order.Contracts.IntegrationEvents;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.IntegrationEventHandlers;

public class OrderCreatedProjectionHandler(OrderSearchMongoDbContext context)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var readModel = new OrderReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Street = @event.Street,
            City = @event.City,
            State = @event.State,
            Country = @event.Country,
            ZipCode = @event.ZipCode,
            Status = @event.Status,
            Currency = @event.Currency,
            CreatedAt = @event.CreatedAt,
            Notes = @event.Notes,
            Version = @event.Version,
            OrderItems = []
        };

        await context.Orders.InsertOneAsync(readModel, cancellationToken: cancellationToken);
    }
}
