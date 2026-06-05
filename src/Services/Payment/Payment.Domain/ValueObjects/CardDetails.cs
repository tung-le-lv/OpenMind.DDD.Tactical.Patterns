using BuildingBlocks.Domain;

namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object representing Card Payment Details.
/// Contains only what's needed for processing - no full card numbers stored.
/// </summary>
public class CardDetails : ValueObject
{
    public string Last4Digits { get; }
    public string CardType { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string CardHolderName { get; }

    private CardDetails() { }

    // Bypasses the expiry-year guard — a card valid at payment time may expire before the record is rehydrated.
    private CardDetails(string last4Digits, string cardType, int expiryMonth, int expiryYear, string cardHolderName, bool _)
    {
        Last4Digits    = last4Digits;
        CardType       = cardType;
        ExpiryMonth    = expiryMonth;
        ExpiryYear     = expiryYear;
        CardHolderName = cardHolderName;
    }

    public static CardDetails Reconstitute(
        string last4Digits, string cardType, int expiryMonth, int expiryYear, string cardHolderName) =>
        new(last4Digits, cardType, expiryMonth, expiryYear, cardHolderName, false);

    public CardDetails(string last4Digits, string cardType, int expiryMonth, int expiryYear, string cardHolderName)
    {
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
        {
            throw new ArgumentException("Last 4 digits must be exactly 4 characters", nameof(last4Digits));
        }

        if (string.IsNullOrWhiteSpace(cardType))
        {
            throw new ArgumentException("Card type is required", nameof(cardType));
        }

        if (expiryMonth < 1 || expiryMonth > 12)
        {
            throw new ArgumentException("Expiry month must be between 1 and 12", nameof(expiryMonth));
        }

        if (expiryYear < DateTime.UtcNow.Year)
        {
            throw new ArgumentException("Card has expired", nameof(expiryYear));
        }

        if (string.IsNullOrWhiteSpace(cardHolderName))
        {
            throw new ArgumentException("Card holder name is required", nameof(cardHolderName));
        }

        Last4Digits    = last4Digits;
        CardType       = cardType;
        ExpiryMonth    = expiryMonth;
        ExpiryYear     = expiryYear;
        CardHolderName = cardHolderName;
    }

    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        return ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Last4Digits;
        yield return CardType;
        yield return ExpiryMonth;
        yield return ExpiryYear;
        yield return CardHolderName;
    }

    public override string ToString() => $"{CardType} ****{Last4Digits}";
}
