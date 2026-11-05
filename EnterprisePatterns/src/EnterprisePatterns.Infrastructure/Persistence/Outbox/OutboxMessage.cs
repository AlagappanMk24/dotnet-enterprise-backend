using EnterprisePatterns.Domain.Common;
using System.Text.Json;

namespace EnterprisePatterns.Infrastructure.Persistence.Outbox;

/// <summary>
/// Row in the outbox table: a serialized domain event waiting to be published.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Transactional Outbox Pattern + Factory Method (<see cref="FromDomainEvent"/>).<br/>
/// <b>Purpose:</b> Stores the event in the SAME database transaction as the business change.<br/>
/// <b>Why It Is Used:</b> Solves the dual-write problem: without it, the order could be saved but the event lost
/// (or the event published but the save rolled back). The event id is the primary key, so consumers can de-duplicate.
/// </remarks>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    /// <summary>Assembly-qualified CLR type name of the event.</summary>
    public string Type { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();
        return new OutboxMessage
        {
            Id = domainEvent.EventId,
            Type = type.AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, type, OutboxJson.Options),
            OccurredOnUtc = domainEvent.OccurredOn.UtcDateTime,
        };
    }
}

/// <summary>
/// Shared serializer settings for the outbox.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Singleton via static readonly field (thread-safe CLR static initialisation) + DRY.<br/>
/// <b>Purpose:</b> Guarantees writer and reader use identical JSON settings.<br/>
/// <b>Why It Is Used:</b> JsonSerializerOptions is expensive to build and is thread safe once in use, so one shared instance is best practice.
/// </remarks>
internal static class OutboxJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}