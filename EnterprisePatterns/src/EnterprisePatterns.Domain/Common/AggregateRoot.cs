namespace EnterprisePatterns.Domain.Common;

/// <summary>
/// Base class for aggregate roots: the only entry point to a consistency boundary.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Aggregate Root (DDD) + Encapsulation + Observer (event recording).<br/>
/// <b>Purpose:</b> Records domain events raised by business methods until Infrastructure persists them.<br/>
/// <b>Why It Is Used:</b> Business invariants are enforced in one place, and side effects are decoupled from the
/// transaction via events instead of being called inline (Single Responsibility).
/// </remarks>
public abstract class AggregateRoot : Entity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Records an event; only the aggregate itself may raise events.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
