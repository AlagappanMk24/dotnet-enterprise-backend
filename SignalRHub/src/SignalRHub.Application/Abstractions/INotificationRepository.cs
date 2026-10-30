using SignalRHub.Domain.Entities;

namespace SignalRHub.Application.Abstractions;

/// <summary>
/// Storage contract for notifications. The Application layer only knows this interface;
/// Infrastructure supplies the EF Core implementation.
/// </summary>
public interface INotificationRepository
{
    /// <summary>Saves a new notification to the database.</summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>
    /// Returns a user's unread notifications, newest first.
    /// Useful for users who were offline and missed the live push.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(string userId, CancellationToken ct = default);
}
