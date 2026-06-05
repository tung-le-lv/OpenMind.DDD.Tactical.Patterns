using MongoDB.Bson.Serialization.Attributes;

namespace Payment.Infrastructure.Persistence.Documents;

public class PaymentDocument
{
    [BsonId]
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal AmountValue { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Method { get; set; } = default!;
    public CardDetailsDocument? CardDetails { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Version { get; set; }
}

public class CardDetailsDocument
{
    public string Last4Digits { get; set; } = default!;
    public string CardType { get; set; } = default!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = default!;
}
