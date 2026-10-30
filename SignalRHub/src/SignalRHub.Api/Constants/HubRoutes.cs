namespace SignalRHub.Api.Constants;

/// <summary>URL paths of SignalR hubs, defined once so they cannot be mistyped.</summary>
public static class HubRoutes
{
    /// <summary>Browsers connect to wss://host/hubs/notifications.</summary>
    public const string Notifications = "/hubs/notifications";
}
