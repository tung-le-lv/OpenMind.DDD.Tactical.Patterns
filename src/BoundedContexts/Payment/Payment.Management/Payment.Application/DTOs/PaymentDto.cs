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
    DateTime? CompletedAt)
{
    public static PaymentDto From(Domain.Aggregates.PaymentAggregate.Payment payment) => new(
        payment.Id.Value,
        payment.OrderId.Value,
        payment.CustomerId.Value,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status.Name,
        payment.Method.Name,
        payment.CardDetails != null
            ? new CardDetailsDto(
                payment.CardDetails.Last4Digits,
                payment.CardDetails.CardType,
                payment.CardDetails.ExpiryMonth,
                payment.CardDetails.ExpiryYear,
                payment.CardDetails.CardHolderName)
            : null,
        payment.TransactionId,
        payment.FailureReason,
        payment.CreatedAt,
        payment.ProcessedAt,
        payment.CompletedAt);
}
