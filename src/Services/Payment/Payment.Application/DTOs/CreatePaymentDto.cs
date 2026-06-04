namespace Payment.Application.DTOs;

public record CreatePaymentDto(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency = "USD",
    string Method = "CreditCard",
    CardDetailsDto? CardDetails = null);
