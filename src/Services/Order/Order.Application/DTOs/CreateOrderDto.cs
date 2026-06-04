namespace Order.Application.DTOs;

public record CreateOrderDto(Guid CustomerId, AddressDto ShippingAddress, string Currency = "USD", string? Notes = null);
