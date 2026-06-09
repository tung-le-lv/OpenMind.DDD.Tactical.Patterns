using MediatR;

namespace Order.Search.Features.SearchOrders;

public record SearchOrdersQuery(
    Guid? CustomerId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    decimal? MinAmount,
    decimal? MaxAmount,
    string? ProductName,
    int Page,
    int PageSize,
    string SortBy,
    bool SortDescending
) : IRequest<SearchOrdersResponse>;

public record SearchOrdersResponse(
    IReadOnlyList<OrderSearchResult> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record OrderSearchResult(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    string City,
    string Country,
    int ItemCount,
    DateTime CreatedAt
);
