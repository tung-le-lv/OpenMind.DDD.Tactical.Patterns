using MediatR;

namespace Order.Search.Features.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(
    Guid CustomerId,
    string? Status,
    int Page,
    int PageSize
) : IRequest<CustomerOrdersResponse>;

public record CustomerOrdersResponse(
    Guid CustomerId,
    IReadOnlyList<OrderSummary> Orders,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record OrderSummary(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAt,
    DateTime? SubmittedAt
);
