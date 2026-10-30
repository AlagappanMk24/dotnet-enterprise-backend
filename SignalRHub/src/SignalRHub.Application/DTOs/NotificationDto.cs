using SignalRHub.Domain.Entities;
using SignalRHub.Domain.Enums;

namespace SignalRHub.Application.DTOs;

/// <summary>
/// The message shape sent to browsers. Small and read-only.
/// Entities never leave the server, only this record does.
/// </summary>
/// <param name="Id">Notification id.</param>
/// <param name="Title">Headline.</param>
/// <param name="Message">Body text.</param>
/// <param name="Severity">Importance level.</param>
/// <param name="CreatedAtUtc">Creation time in UTC.</param>
public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationSeverity Severity,
    DateTimeOffset CreatedAtUtc);

/// <summary>Conversion helpers between the entity and the DTO.</summary>
public static class NotificationMappings
{
    /// <summary>Copies the entity fields that clients are allowed to see.</summary>
    public static NotificationDto ToDto(this Notification n) =>
        new(n.Id, n.Title, n.Message, n.Severity, n.CreatedAtUtc);
}
