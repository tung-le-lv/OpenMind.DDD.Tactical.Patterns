namespace Order.Application.DTOs;

public record AddOrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
