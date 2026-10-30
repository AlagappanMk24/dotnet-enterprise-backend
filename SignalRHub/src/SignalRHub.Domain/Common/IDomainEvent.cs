namespace SignalRHub.Domain.Common;

/// <summary>
/// Marker for "something important happened in the business".
/// Entities record these events; a dispatcher (not built yet) can publish them later,
/// for example to write an audit log or trigger an email.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the event happened, in UTC.</summary>
    DateTimeOffset OccurredOnUtc { get; }
}
