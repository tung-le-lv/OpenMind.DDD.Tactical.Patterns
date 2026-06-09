using MediatR;

namespace Order.Search.Features.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailResponse?>;

public record OrderDetailResponse(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    ShippingAddressResponse ShippingAddress,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    DateTime? SubmittedAt,
    DateTime? PaidAt
);

public record ShippingAddressResponse(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode
);

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount,
    decimal LineTotal,
    string Currency
);
