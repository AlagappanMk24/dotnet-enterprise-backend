using SignalRHub.Domain.Common;

namespace SignalRHub.Domain.Events;

/// <summary>
/// Raised inside <c>Notification.Create</c> when a new notification is built.
/// Today it is only recorded on the entity; nothing consumes it yet.
/// </summary>
/// <param name="NotificationId">Id of the notification that was created.</param>
public sealed record NotificationCreatedDomainEvent(Guid NotificationId) : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
