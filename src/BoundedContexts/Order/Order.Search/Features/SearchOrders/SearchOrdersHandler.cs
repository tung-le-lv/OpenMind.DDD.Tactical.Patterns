using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.Features.SearchOrders;

public class SearchOrdersHandler(OrderSearchMongoDbContext context)
    : IRequestHandler<SearchOrdersQuery, SearchOrdersResponse>
{
    public async Task<SearchOrdersResponse> Handle(
        SearchOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var collection = context.Orders;

        // Compute totalAmount server-side: sum(unitPrice * qty - discount) per item
        var addTotalAmount = new BsonDocument("$addFields", new BsonDocument("totalAmount",
            new BsonDocument("$sum",
                new BsonDocument("$map", new BsonDocument
                {
                    { "input", "$orderItems" },
                    { "as", "item" },
                    {
                        "in", new BsonDocument("$subtract", new BsonArray
                        {
                            new BsonDocument("$multiply", new BsonArray { "$$item.unitPriceAmount", "$$item.quantity" }),
                            "$$item.discountAmount"
                        })
                    }
                })
            )
        ));

        // Render typed FilterDefinition to BsonDocument so MongoDB driver handles GUID serialization
        var typedFilter = BuildTypedFilter(query);
        var serializer = BsonSerializer.SerializerRegistry.GetSerializer<OrderReadModel>();
        var filterDoc = typedFilter.Render(new(serializer, BsonSerializer.SerializerRegistry));

        // Add amount range to the rendered filter document
        if (query.MinAmount.HasValue || query.MaxAmount.HasValue)
        {
            var amountRange = new BsonDocument();
            if (query.MinAmount.HasValue)
                amountRange.Add("$gte", new BsonDecimal128(query.MinAmount.Value));
            if (query.MaxAmount.HasValue)
                amountRange.Add("$lte", new BsonDecimal128(query.MaxAmount.Value));
            filterDoc.Add("totalAmount", amountRange);
        }

        var matchStage = new BsonDocument("$match", filterDoc);

        var sortField = query.SortBy switch
        {
            "TotalAmount" => "totalAmount",
            "Status"      => "status",
            _             => "createdAt"
        };
        var sortStage = new BsonDocument("$sort",
            new BsonDocument(sortField, query.SortDescending ? -1 : 1));

        // $facet runs count + paginated data in a single round-trip
        var facetStage = new BsonDocument("$facet", new BsonDocument
        {
            {
                "metadata", new BsonArray
                {
                    new BsonDocument("$count", "total")
                }
            },
            {
                "data", new BsonArray
                {
                    sortStage,
                    new BsonDocument("$skip", (query.Page - 1) * query.PageSize),
                    new BsonDocument("$limit", query.PageSize),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 1 },
                        { "customerId", 1 },
                        { "status", 1 },
                        { "totalAmount", 1 },
                        { "currency", 1 },
                        { "city", 1 },
                        { "country", 1 },
                        { "createdAt", 1 },
                        { "itemCount", new BsonDocument("$size", "$orderItems") }
                    })
                }
            }
        });

        var pipeline = new[] { addTotalAmount, matchStage, facetStage };

        var facetResult = await collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (facetResult is null)
            return new SearchOrdersResponse([], 0, query.Page, query.PageSize, 0);

        var totalCount = facetResult["metadata"].AsBsonArray.Count > 0
            ? facetResult["metadata"][0]["total"].AsInt32
            : 0;

        var items = facetResult["data"].AsBsonArray
            .Select(doc => new OrderSearchResult(
                doc["_id"].AsGuid,
                doc["customerId"].AsGuid,
                doc["status"].AsString,
                doc["totalAmount"].ToDecimal(),
                doc["currency"].AsString,
                doc["city"].AsString,
                doc["country"].AsString,
                doc["itemCount"].AsInt32,
                doc["createdAt"].ToUniversalTime()))
            .ToList();

        return new SearchOrdersResponse(
            items,
            totalCount,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }

    private static FilterDefinition<OrderReadModel> BuildTypedFilter(SearchOrdersQuery query)
    {
        var f = Builders<OrderReadModel>.Filter;
        var filters = new List<FilterDefinition<OrderReadModel>>();

        if (query.CustomerId.HasValue)
            filters.Add(f.Eq(o => o.CustomerId, query.CustomerId.Value));

        if (!string.IsNullOrWhiteSpace(query.Status))
            filters.Add(f.Eq(o => o.Status, query.Status));

        if (query.FromDate.HasValue)
            filters.Add(f.Gte(o => o.CreatedAt, query.FromDate.Value));

        if (query.ToDate.HasValue)
            filters.Add(f.Lte(o => o.CreatedAt, query.ToDate.Value));

        if (!string.IsNullOrWhiteSpace(query.ProductName))
            filters.Add(f.Text(query.ProductName));

        return filters.Count > 0 ? f.And(filters) : f.Empty;
    }
}
