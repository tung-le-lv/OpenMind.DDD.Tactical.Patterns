using BuildingBlocks.Domain;

namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object representing Money in the Payment domain.
/// Note: This is a separate Money class from Order domain - each Bounded Context
/// has its own models even if they look similar. This maintains context autonomy.
/// </summary>
public class Money : ValueObject
{
    // Payment processors (e.g. Stripe) reject charges below this threshold.
    public const decimal MinimumChargeableAmount = 0.50m;

    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required", nameof(currency));
        }

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0, currency);
    public static Money FromDecimal(decimal amount, string currency = "USD") => new(amount, currency);

    /// Returns true when this amount meets the minimum a payment processor will accept.
    public bool IsChargeable() => Amount >= MinimumChargeableAmount;

    /// Converts to integer minor units required by payment gateways (e.g. 1999 for $19.99).
    public long ToMinorUnits() => (long)Math.Round(Amount * 100, MidpointRounding.AwayFromZero);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
