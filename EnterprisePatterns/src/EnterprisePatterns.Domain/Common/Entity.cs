namespace EnterprisePatterns.Domain.Common;

/// <summary>
/// Base class for objects that have an identity which outlives their attribute values.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Entity (DDD tactical pattern) + identity-based equality (DRY).<br/>
/// <b>Purpose:</b> Two entities are equal when they are the same type and have the same Id, regardless of other state.<br/>
/// <b>Why It Is Used:</b> Guarantees consistent equality semantics in collections, caches and the EF change tracker,
/// and avoids duplicating Equals/GetHashCode in every entity.
/// </remarks>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>Identity. Generated client-side (GUID v7) so aggregates have an id before they are saved.</summary>
    public Guid Id { get; protected set; }

    public bool Equals(Entity? other) =>
        other is not null && GetType() == other.GetType() && Id != Guid.Empty && Id == other.Id;

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}