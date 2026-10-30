using SignalRHub.Application.DTOs;

namespace SignalRHub.Application.Abstractions;

/// <summary>
/// The "push a live message" contract. Handlers and background workers call this
/// and never see SignalR. The implementation lives in the API project
/// (SignalRNotificationService) because the hub type lives there.
/// </summary>
public interface INotificationService
{
    /// <summary>Sends to every open connection of one user (all their tabs and devices).</summary>
    /// <param name="userId">The user's id, which matches the "sub" claim in their JWT.</param>
    /// <param name="notification">The message to deliver.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken ct = default);

    /// <summary>Sends to everyone currently in a group (a room).</summary>
    /// <param name="group">Full group name, built with NotificationGroups.Room(...).</param>
    /// <param name="notification">The message to deliver.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendToGroupAsync(string group, NotificationDto notification, CancellationToken ct = default);

    /// <summary>Sends to every connected client.</summary>
    /// <param name="notification">The message to deliver.</param>
    /// <param name="ct">Cancellation token.</param>
    Task BroadcastAsync(NotificationDto notification, CancellationToken ct = default);
}
