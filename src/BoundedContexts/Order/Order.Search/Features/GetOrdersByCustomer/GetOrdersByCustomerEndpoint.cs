using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Order.Search.Features.GetOrdersByCustomer;

public static class GetOrdersByCustomerEndpoint
{
    public static void MapGetOrdersByCustomer(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/customer/{customerId:guid}", async (
                Guid customerId,
                [FromQuery] string? status,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetOrdersByCustomerQuery(
                    customerId,
                    status,
                    page <= 0 ? 1 : page,
                    pageSize is <= 0 or > 100 ? 20 : pageSize);

                var result = await mediator.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetOrdersByCustomer")
            .WithSummary("Get paginated orders for a customer, optionally filtered by status")
            .WithTags("Orders")
            .Produces<CustomerOrdersResponse>();
    }
}
