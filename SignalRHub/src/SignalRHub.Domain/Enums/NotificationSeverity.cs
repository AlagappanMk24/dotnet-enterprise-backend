namespace SignalRHub.Domain.Enums;

/// <summary>
/// How important a notification is. Browsers use it to pick a color or icon.
/// Travels over HTTP and SignalR as text ("Info", "Critical") because
/// JsonStringEnumConverter is registered in the API layer.
/// </summary>
public enum NotificationSeverity
{
    /// <summary>General information, nothing to act on.</summary>
    Info = 0,

    /// <summary>Something finished successfully.</summary>
    Success = 1,

    /// <summary>Needs attention soon.</summary>
    Warning = 2,

    /// <summary>Needs attention now.</summary>
    Critical = 3
}
