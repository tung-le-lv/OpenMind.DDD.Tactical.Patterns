namespace Order.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    string? Notes,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? PaidAt);
