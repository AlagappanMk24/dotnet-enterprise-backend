using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Application.Abstractions;
using SignalRHub.Application.Common;

namespace SignalRHub.Api.Hubs;

/// <summary>
/// The SignalR endpoint browsers connect to. It does two jobs:
/// 1) handles calls FROM clients (JoinRoom, LeaveRoom), and
/// 2) reacts to connections opening and closing.
/// It does NOT send notifications. That is done by SignalRNotificationService.
/// [Authorize] means only requests with a valid JWT can connect.
/// </summary>
[Authorize]
public sealed partial class NotificationHub(
    IConnectionTracker tracker,
    ILogger<NotificationHub> logger) : Hub<INotificationClient>
{
    // Room ids may only contain letters, digits, "_" and "-" (max 64).
    // This stops clients from inventing odd group names.
    [GeneratedRegex("^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex RoomIdRegex();

    /// <summary>Runs once each time a client connects (after authentication succeeded).</summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        // Remember this connection so we can tell who is online.
        tracker.Add(userId, Context.ConnectionId);

        logger.LogInformation(
            "Connected {ConnectionId} for user {UserId}. Total: {Total}",
            Context.ConnectionId, userId, tracker.TotalConnections);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Runs once each time a client disconnects. SignalR removes the connection from
    /// all groups by itself, so only our own tracker needs cleaning.
    /// </summary>
    /// <param name="exception">Null for a normal close; set when the connection dropped because of an error.</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is { } userId)
            tracker.Remove(userId, Context.ConnectionId);

        if (exception is null)
            logger.LogInformation("Disconnected {ConnectionId}", Context.ConnectionId);
        else
            logger.LogWarning(exception, "Connection {ConnectionId} dropped", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Called by the browser: connection.invoke("JoinRoom", "room-101").
    /// Subscribes this connection to the room so it receives messages sent to it.
    /// </summary>
    /// <param name="roomId">Room id such as "room-101" (not the full group name).</param>
    /// <exception cref="HubException">The room id is invalid.</exception>
    public async Task JoinRoom(string roomId)
    {
        EnsureValidRoomId(roomId);
        var group = NotificationGroups.Room(roomId);   // "room-101" -> "room:room-101"

        await Groups.AddToGroupAsync(Context.ConnectionId, group);

        // Tell the other members, but not the caller.
        await Clients.OthersInGroup(group).UserJoinedRoom(roomId, GetUserId());
    }

    /// <summary>Called by the browser to stop receiving a room's messages.</summary>
    /// <param name="roomId">Room id such as "room-101".</param>
    /// <exception cref="HubException">The room id is invalid.</exception>
    public async Task LeaveRoom(string roomId)
    {
        EnsureValidRoomId(roomId);
        var group = NotificationGroups.Room(roomId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        await Clients.OthersInGroup(group).UserLeftRoom(roomId, GetUserId());
    }

    /// <summary>
    /// The current user's id, taken from the JWT "sub" claim by SubClaimUserIdProvider.
    /// </summary>
    private string GetUserId() =>
        Context.UserIdentifier ?? throw new HubException("Authenticated user id is missing.");

    /// <summary>Throws a HubException (shown to the client as an error) when the room id is not allowed.</summary>
    private static void EnsureValidRoomId(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId) || !RoomIdRegex().IsMatch(roomId))
            throw new HubException("Invalid room id.");
    }
}
