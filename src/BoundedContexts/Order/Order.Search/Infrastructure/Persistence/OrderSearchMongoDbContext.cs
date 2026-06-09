using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Order.Search.ReadModels;

namespace Order.Search.Infrastructure.Persistence;

public class OrderSearchMongoDbContext
{
    private readonly IMongoDatabase _database;

    public OrderSearchMongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<OrderReadModel> Orders =>
        _database.GetCollection<OrderReadModel>("orders");

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var col = Orders;
        var keys = Builders<OrderReadModel>.IndexKeys;

        var indexes = new[]
        {
            // Customer lookups
            new CreateIndexModel<OrderReadModel>(
                keys.Ascending(o => o.CustomerId)),

            // Status filtering
            new CreateIndexModel<OrderReadModel>(
                keys.Ascending(o => o.Status)),

            // Common compound: customer + status (covers GetOrdersByCustomer with status filter)
            new CreateIndexModel<OrderReadModel>(
                keys.Ascending(o => o.CustomerId).Ascending(o => o.Status)),

            // Date range sorting and filtering
            new CreateIndexModel<OrderReadModel>(
                keys.Descending(o => o.CreatedAt)),

            // Compound for the most frequent search pattern: status + createdAt
            new CreateIndexModel<OrderReadModel>(
                keys.Ascending(o => o.Status).Descending(o => o.CreatedAt)),

            // Full-text search on product names inside the nested array
            new CreateIndexModel<OrderReadModel>(
                keys.Text("orderItems.productName"),
                new CreateIndexOptions { Name = "order_items_text" })
        };

        await col.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
