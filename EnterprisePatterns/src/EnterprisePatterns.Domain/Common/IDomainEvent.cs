namespace EnterprisePatterns.Domain.Common;

/// <summary>
/// Contract for something meaningful that happened inside the domain.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Domain Event (DDD) + Observer Pattern (publisher side) + Interface Segregation Principle.<br/>
/// <b>Purpose:</b> Gives every event a unique id and timestamp so it can be persisted, de-duplicated and replayed.<br/>
/// <b>Why It Is Used:</b> Aggregates announce facts ("order placed") without knowing who reacts. This removes coupling
/// between business rules and side effects (emails, caches, integrations) and keeps the Domain dependency-free.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>Unique id; also used as the outbox primary key so consumers can be idempotent.</summary>
    Guid EventId { get; }

    /// <summary>UTC moment the fact occurred.</summary>
    DateTimeOffset OccurredOn { get; }
}