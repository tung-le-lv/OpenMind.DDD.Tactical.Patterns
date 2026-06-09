using BuildingBlocks.Domain;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Standalone Class (Evans ch.10)
/// ─────────────────────────────
/// A class is "standalone" when it can be understood and tested without referencing
/// any other domain class. 
/// 
/// When you can, go all the way. Eliminate all other concepts from the picture. 
/// Then the class will be completely self-contained and can be studied and understood alone.
/// Try to factor the most intricate computations into STANDALONE CLASSES, 
/// perhaps by modeling VALUE OBJECTS held by the more connected classes.
/// 
/// ─────────────────────────────────────
/// This Percentage class imports nothing from the Order domain —
/// only the ValueObject base from BuildingBlocks and .NET primitives.
/// Every constraint it enforces (0–100 range, non-negative arithmetic) is defined
/// right here. A reader never needs to open another file to reason about it.
///
/// Closure of Operations (Evans ch.10)
/// ─────────────────────────────────────
/// An operation is "closed" when its return type equals the type of its argument(s),
/// just as integer addition is closed over integers: int + int → int.
/// Every method below that takes a Percentage returns a Percentage, keeping all
/// composition within the type. Callers can chain freely:
///
///   var net = base.Add(loyalty).Cap(Percentage.Of(50));
///   var discountAmount = net.ApplyTo(orderTotal);
///
/// without ever leaving the Percentage concept or converting to raw decimals.
/// The decimal-level arithmetic stays inside this class; the rest of the model
/// works purely with Percentage values.
/// </summary>
public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage() { }

    private Percentage(decimal value) => Value = value;

    public static Percentage Of(decimal value)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Percentage must be between 0 and 100, got {value}.");
        }
        
        return new Percentage(value);
    }

    public static Percentage Zero => new(0m);
    public static Percentage Full => new(100m);

    // ── Closure of Operations ────────────────────────────────────────────────
    // Every operation below takes a Percentage and returns a Percentage.
    // The type is "closed" over these operations — no escape to raw decimals.

    public Percentage Add(Percentage other) => new(Math.Min(100m, Value + other.Value));

    public Percentage Subtract(Percentage other) => new(Math.Max(0m, Value - other.Value));

    public Percentage Complement() => new(100m - Value);

    public Percentage Cap(Percentage ceiling) => new(Math.Min(Value, ceiling.Value));
    
    public decimal ApplyTo(decimal amount) => amount * Value / 100m;

    public bool IsZero => Value == 0m;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:G29}%";
}
