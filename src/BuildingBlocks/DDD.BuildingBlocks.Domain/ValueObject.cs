namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for Value Objects in the domain.
/// A Value Object is defined by its attributes rather than identity (Eric Evans, DDD).
/// Value Objects are immutable - once created, their state cannot change.
/// Two Value Objects are equal if all their attributes are equal.
/// 
/// In .NET, we prefer to use record type or strongly type for value object.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the atomic values that define equality for this Value Object.
    /// Override this method to specify which properties should be compared.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a copy of this Value Object.
    /// Since Value Objects are immutable, this typically returns a new instance with the same values.
    /// </summary>
    public ValueObject GetCopy()
    {
        return (ValueObject)MemberwiseClone();
    }
}
