using MediatR;

namespace Order.Search.Features.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapGetOrderById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{orderId:guid}", async (
                Guid orderId,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetOrderByIdQuery(orderId), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetOrderById")
            .WithSummary("Get full order detail by ID")
            .WithTags("Orders")
            .Produces<OrderDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
