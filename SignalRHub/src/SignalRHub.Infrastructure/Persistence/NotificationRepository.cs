using Microsoft.EntityFrameworkCore;
using SignalRHub.Application.Abstractions;
using SignalRHub.Domain.Entities;

namespace SignalRHub.Infrastructure.Persistence;

/// <summary>EF Core implementation of INotificationRepository.</summary>
public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    /// <inheritdoc />
    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Add(notification);

        // The INSERT happens here. In a larger system a Unit of Work would own this call.
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(
        string userId, CancellationToken ct = default) =>
        await db.Notifications
            .AsNoTracking()                                   // read-only query, skip change tracking
            .Where(n => n.TargetUserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAtUtc)           // newest first
            .ToListAsync(ct);
}
