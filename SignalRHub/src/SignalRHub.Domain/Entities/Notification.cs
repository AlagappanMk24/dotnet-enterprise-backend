using SignalRHub.Domain.Common;
using SignalRHub.Domain.Enums;
using SignalRHub.Domain.Events;

namespace SignalRHub.Domain.Entities;

/// <summary>
/// A notification as the business understands it. This is the row saved in the database.
/// It is NOT what browsers receive (they get a NotificationDto), so the database shape
/// and the wire shape can change independently.
/// </summary>
public sealed class Notification
{
    // Events raised by this entity. Kept in memory until something dispatches them.
    private readonly List<IDomainEvent> _domainEvents = [];

    // Private on purpose: nobody can build a half-valid Notification with "new".
    // EF Core can still use it when it loads rows. In your own code call Create(...).
    private Notification() { }

    /// <summary>Unique id, generated when the notification is created.</summary>
    public Guid Id { get; private set; }

    /// <summary>Short headline shown to the user.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Longer text shown under the title.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Importance level (Info, Success, Warning, Critical).</summary>
    public NotificationSeverity Severity { get; private set; }

    /// <summary>User id this was sent to. Null unless the target was "one user".</summary>
    public string? TargetUserId { get; private set; }

    /// <summary>Group name this was sent to, for example "room:room-101". Null unless the target was a room.</summary>
    public string? TargetGroup { get; private set; }

    /// <summary>Creation time in UTC.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>False until the user reads it. Used later for an "unread" list.</summary>
    public bool IsRead { get; private set; }

    /// <summary>Events recorded by this entity (read-only view).</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// The only way to create a Notification. It checks the rules, fills the id and timestamp,
    /// and records a NotificationCreatedDomainEvent.
    /// </summary>
    /// <exception cref="ArgumentException">Title or message is empty or whitespace.</exception>
    public static Notification Create(
        string title,
        string message,
        NotificationSeverity severity,
        string? targetUserId,
        string? targetGroup)
    {
        // Business rules: a notification without text makes no sense.
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Message = message.Trim(),
            Severity = severity,
            TargetUserId = targetUserId,
            TargetGroup = targetGroup,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        // Record "this was created". Not published anywhere yet.
        notification._domainEvents.Add(new NotificationCreatedDomainEvent(notification.Id));
        return notification;
    }

    /// <summary>Marks the notification as read.</summary>
    public void MarkAsRead() => IsRead = true;

    /// <summary>Clears recorded events, normally after they were dispatched.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
