namespace EnterprisePatterns.Domain.Common;

/// <summary>
/// Base class for immutable objects defined purely by their values.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Value Object (DDD) + Template Method (GoF Behavioral): subclasses only supply
/// <see cref="GetEqualityComponents"/>, the base class owns the equality algorithm.<br/>
/// <b>Purpose:</b> Structural equality for Money, Address and similar concepts.<br/>
/// <b>Why It Is Used:</b> Removes primitive obsession, makes invalid states unrepresentable and eliminates
/// copy-pasted equality code, a common source of subtle production bugs.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>The ordered values that define equality (the "hook" of the Template Method).</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetType() == other.GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}