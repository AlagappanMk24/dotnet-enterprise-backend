namespace SignalRHub.Application.Common;

/// <summary>
/// The single place that decides SignalR group names.
/// The hub (when a client joins) and the handler (when it sends) both use it,
/// so the two sides can never disagree on a name.
/// </summary>
public static class NotificationGroups
{
    /// <summary>Room id used by the background worker for status pings.</summary>
    public const string SystemStatusRoomId = "system-status";

    /// <summary>Turns a room id such as "room-101" into the group name "room:room-101".</summary>
    public static string Room(string roomId) => $"room:{roomId}";
}
