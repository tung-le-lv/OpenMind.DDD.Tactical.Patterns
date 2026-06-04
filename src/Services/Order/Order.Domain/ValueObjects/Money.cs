using BuildingBlocks.Domain;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Value Object representing Money in the Order domain.
///
/// Supple Design patterns applied:
/// - Standalone Class: depends only on ValueObject and Percentage — both standalone value objects
///   with no aggregate or service dependencies. No external domain knowledge is needed to reason about it.
/// - Closure of Operations: every arithmetic operation returns Money → Money is closed under its operations.
///   ApplyDiscount(Percentage) → Money demonstrates closure between two standalone types.
/// - Side-Effect-Free Functions (Evans): Add, Subtract, Multiply, ApplyDiscount are the canonical
///   demonstration. Each takes input values and returns a NEW Money instance — nothing is mutated.
///   Because this Value Object is immutable, ALL of its operations are structurally side-effect-free:
///   the compiler enforces the guarantee Evans asks you to state explicitly. Callers can compose chains
///   like price.ApplyDiscount(10).Add(tax) freely, without fear of changing state elsewhere.
/// - Intention-Revealing Interfaces: IsZero, IsGreaterThanOrEqualTo, ApplyDiscount express
///   domain intent rather than raw arithmetic
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { } // for persistence mappers

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    // ── Intention-Revealing factory methods ──────────────────────────────────

    public static Money Zero(string currency = "USD") => new(0, currency);
    public static Money FromDecimal(decimal amount, string currency = "USD") => new(amount, currency);

    // ── Intention-Revealing query predicates ────────────────────────────────

    /// Returns true when the amount carries no monetary value.
    public bool IsZero => Amount == 0;

    public bool IsGreaterThanOrEqualTo(Money threshold)
    {
        EnsureSameCurrency(threshold);
        return Amount >= threshold.Amount;
    }

    // ── Closure of Operations ─────────────────────────────────────────────────
    // Every operation takes Money and returns Money, keeping the type closed.
    // Callers can compose chains like: price.ApplyDiscount(10).Add(tax)
    // without leaving the Money concept.

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Resulting amount cannot be negative");
        return new Money(result, Currency);
    }

    public Money Multiply(int multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));
        return new Money(Amount * multiplier, Currency);
    }

    /// Reduces this amount by a percentage and returns a new Money — result stays a Money.
    /// Closure of Operations: Money.ApplyDiscount(Percentage) → Money.
    /// The Percentage type already enforces the 0–100 invariant, so Money needs no guard here.
    public Money ApplyDiscount(Percentage discount)
        => new(Amount - discount.ApplyTo(Amount), Currency);

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

    // ── Standalone: private helper keeps the currency guard inside Money itself ─

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }
}
