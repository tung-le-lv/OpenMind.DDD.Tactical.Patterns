using BuildingBlocks.Domain;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Value Object for a customer's email address.
/// Enforces format invariants at construction — if an Email exists, it is valid.
/// </summary>
public class Email : ValueObject
{
    public string Value { get; }

    private Email() { }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required", nameof(value));
        }

        if (!value.Contains('@') || !value.Contains('.'))
        {
            throw new ArgumentException($"'{value}' is not a valid email address", nameof(value));
        }

        Value = value.Trim().ToLowerInvariant();
    }

    public static Email From(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
