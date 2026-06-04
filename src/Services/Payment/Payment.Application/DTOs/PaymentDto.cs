namespace Payment.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    CardDetailsDto? CardDetails,
    string? TransactionId,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    DateTime? CompletedAt);
