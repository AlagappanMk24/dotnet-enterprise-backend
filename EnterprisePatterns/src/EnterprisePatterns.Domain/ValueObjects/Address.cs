using EnterprisePatterns.Domain.Common;
using EnterprisePatterns.Domain.Exceptions;

namespace EnterprisePatterns.Domain.ValueObjects;

/// <summary>
/// Postal address used as the shipping destination.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Value Object + Factory Method + Self-validating object.<br/>
/// <b>Purpose:</b> Groups street/city/postcode/country into one validated concept.<br/>
/// <b>Why It Is Used:</b> An Address that exists is always valid, so downstream code never re-validates it.
/// </remarks>
public sealed class Address : ValueObject
{
    private Address()
    {
        Street = City = PostalCode = Country = string.Empty;
    }

    private Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public string Street { get; private set; }

    public string City { get; private set; }

    public string PostalCode { get; private set; }

    public string Country { get; private set; }

    public static Address Create(string? street, string? city, string? postalCode, string? country)
    {
        return new Address(
            Require(street, nameof(street), 200),
            Require(city, nameof(city), 100),
            Require(postalCode, nameof(postalCode), 20),
            Require(country, nameof(country), 100));
    }

    private static string Require(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"Address {field} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"Address {field} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}
