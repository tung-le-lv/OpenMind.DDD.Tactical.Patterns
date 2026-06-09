using MediatR;
using MongoDB.Driver;
using Order.Search.Infrastructure.Persistence;
using Order.Search.ReadModels;

namespace Order.Search.Features.GetOrdersByCustomer;

public class GetOrdersByCustomerHandler(OrderSearchMongoDbContext context)
    : IRequestHandler<GetOrdersByCustomerQuery, CustomerOrdersResponse>
{
    public async Task<CustomerOrdersResponse> Handle(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        var builder = Builders<OrderReadModel>.Filter;
        var filter = builder.Eq(o => o.CustomerId, query.CustomerId);

        if (!string.IsNullOrWhiteSpace(query.Status))
            filter &= builder.Eq(o => o.Status, query.Status);

        var collection = context.Orders;

        var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var orders = await collection
            .Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        var summaries = orders
            .Select(o => new OrderSummary(
                o.Id,
                o.Status,
                o.TotalAmount,
                o.Currency,
                o.OrderItems.Count,
                o.CreatedAt,
                o.SubmittedAt))
            .ToList();

        return new CustomerOrdersResponse(
            query.CustomerId,
            summaries,
            (int)totalCount,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }
}
