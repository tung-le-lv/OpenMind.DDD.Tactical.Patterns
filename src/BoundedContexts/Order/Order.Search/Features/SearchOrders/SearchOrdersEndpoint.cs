using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Order.Search.Features.SearchOrders;

public static class SearchOrdersEndpoint
{
    public static void MapSearchOrders(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/search", async (
                [FromQuery] Guid? customerId,
                [FromQuery] string? status,
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                [FromQuery] decimal? minAmount,
                [FromQuery] decimal? maxAmount,
                [FromQuery] string? productName,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? sortBy,
                [FromQuery] bool? sortDescending,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new SearchOrdersQuery(
                    customerId,
                    status,
                    fromDate,
                    toDate,
                    minAmount,
                    maxAmount,
                    productName,
                    page <= 0 ? 1 : page,
                    pageSize is <= 0 or > 100 ? 20 : pageSize,
                    sortBy ?? "CreatedAt",
                    sortDescending ?? true);

                var result = await mediator.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("SearchOrders")
            .WithSummary("Search orders with filtering, sorting, and pagination")
            .WithDescription(
                "Supports filtering by customer, status, date range, amount range, and product name full-text search. " +
                "TotalAmount is computed server-side via MongoDB aggregation pipeline.")
            .WithTags("Orders");
    }
}
