using MediatR;
using MongoDB.Driver;
using Order.Search.Infrastructure.Persistence;

namespace Order.Search.Features.GetOrderById;

public class GetOrderByIdHandler(OrderSearchMongoDbContext context)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailResponse?>
{
    public async Task<OrderDetailResponse?> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Find(o => o.Id == query.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null) return null;

        return new OrderDetailResponse(
            order.Id,
            order.CustomerId,
            order.Status,
            order.TotalAmount,
            order.Currency,
            new ShippingAddressResponse(order.Street, order.City, order.State, order.Country, order.ZipCode),
            order.OrderItems.Select(i => new OrderItemResponse(
                i.ProductId,
                i.ProductName,
                i.UnitPriceAmount,
                i.Quantity,
                i.DiscountAmount,
                i.LineTotal,
                i.Currency)).ToList(),
            order.CreatedAt,
            order.ModifiedAt,
            order.SubmittedAt,
            order.PaidAt);
    }
}
