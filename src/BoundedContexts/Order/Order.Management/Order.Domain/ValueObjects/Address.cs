using BuildingBlocks.Domain;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Value Object representing a shipping address.
/// Immutable and compared by its attributes.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string ZipCode { get; }

    private Address() { }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street is required", nameof(street));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country is required", nameof(country));
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("ZipCode is required", nameof(zipCode));
        }

        Street = street;
        City = city;
        State = state ?? string.Empty;
        Country = country;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
