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
    DateTime? PaidAt)
{
    public static OrderDto From(Domain.Aggregates.OrderAggregate.Order order) => new(
        order.Id.Value,
        order.CustomerId.Value,
        order.Status.Name,
        order.TotalAmount.Amount,
        order.Currency,
        new AddressDto(
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.Country,
            order.ShippingAddress.ZipCode),
        order.OrderItems.Select(item => new OrderItemDto(
            item.Id.Value,
            item.ProductId.Value,
            item.ProductName,
            item.UnitPrice.Amount,
            item.Quantity,
            item.Discount.Amount,
            item.Total.Amount)).ToList(),
        order.Notes,
        order.CreatedAt,
        order.SubmittedAt,
        order.PaidAt);
}
