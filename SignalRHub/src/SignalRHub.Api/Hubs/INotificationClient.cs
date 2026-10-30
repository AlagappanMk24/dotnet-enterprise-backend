using SignalRHub.Application.DTOs;

namespace SignalRHub.Api.Hubs;

/// <summary>
/// Everything the SERVER can call on a CLIENT. Each method name must match a
/// connection.on("MethodName", ...) handler in the browser.
/// Using an interface (Hub&lt;INotificationClient&gt;) gives compile-time checking instead of magic strings.
/// </summary>
public interface INotificationClient
{
    /// <summary>A notification was sent to this client. Browser handler: "ReceiveNotification".</summary>
    Task ReceiveNotification(NotificationDto notification);

    /// <summary>Another user joined a room this client is in. Browser handler: "UserJoinedRoom".</summary>
    Task UserJoinedRoom(string roomId, string userId);

    /// <summary>Another user left a room this client is in. Browser handler: "UserLeftRoom".</summary>
    Task UserLeftRoom(string roomId, string userId);
}
