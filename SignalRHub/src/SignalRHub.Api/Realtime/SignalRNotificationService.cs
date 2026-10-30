using Microsoft.AspNetCore.SignalR;
using SignalRHub.Api.Hubs;
using SignalRHub.Application.Abstractions;
using SignalRHub.Application.DTOs;

namespace SignalRHub.Api.Realtime;

/// <summary>
/// The adapter that makes INotificationService real by using SignalR.
/// It is the ONLY class that knows live messages travel over SignalR. It lives in the API
/// project because NotificationHub lives here, and Infrastructure may not reference the API.
/// IHubContext lets server code push to clients from outside a hub method
/// (from a handler, a worker, a controller).
/// </summary>
public sealed class SignalRNotificationService(
    IHubContext<NotificationHub, INotificationClient> hubContext) : INotificationService
{
    // Note: the typed client methods do not accept a CancellationToken, so "ct" is accepted
    // for the interface but not used. SignalR queues the message and returns quickly.

    /// <inheritdoc />
    public Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken ct = default) =>
        hubContext.Clients.User(userId).ReceiveNotification(notification);

    /// <inheritdoc />
    public Task SendToGroupAsync(string group, NotificationDto notification, CancellationToken ct = default) =>
        hubContext.Clients.Group(group).ReceiveNotification(notification);

    /// <inheritdoc />
    public Task BroadcastAsync(NotificationDto notification, CancellationToken ct = default) =>
        hubContext.Clients.All.ReceiveNotification(notification);
}
