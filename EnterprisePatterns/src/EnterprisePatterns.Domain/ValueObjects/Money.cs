using EnterprisePatterns.Domain.Common;
using EnterprisePatterns.Domain.Exceptions;

namespace EnterprisePatterns.Domain.ValueObjects;

/// <summary>
/// Monetary amount bound to a currency. Immutable and always non-negative.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Value Object + Factory Method (GoF Creational: <see cref="Create"/>) + Immutability.<br/>
/// <b>Purpose:</b> Prevents adding rupees to dollars, negative prices and rounding drift.<br/>
/// <b>Why It Is Used:</b> Money bugs are expensive. Centralising rounding (2 dp, away from zero) and currency checks
/// guarantees every calculation in the system behaves identically.
/// </remarks>
public sealed class Money : ValueObject
{
    // Required by EF Core materialisation only.
    private Money() => Currency = string.Empty;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    /// <summary>Factory Method: the only way to build a valid instance.</summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Money amount cannot be negative.");
        }

        return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero), NormalizeCurrency(currency));
    }

    public static Money Zero(string currency) => Create(0m, currency);

    /// <summary>Validates a 3-letter ISO-4217 style code and upper-cases it.</summary>
    public static string NormalizeCurrency(string? currency)
    {
        var trimmed = currency?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length != 3 || !trimmed.All(char.IsAsciiLetter))
        {
            throw new DomainException("Currency must be a 3-letter ISO code (for example INR, USD).");
        }

        return trimmed.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new DomainException("Multiplier cannot be negative.");
        }

        return Create(Amount * factor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new DomainException($"Currency mismatch: {Currency} vs {other.Currency}.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
