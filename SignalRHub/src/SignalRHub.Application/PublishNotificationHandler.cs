using Microsoft.Extensions.Logging;
using SignalRHub.Application.Abstractions;
using SignalRHub.Application.Common;
using SignalRHub.Application.DTOs;
using SignalRHub.Domain.Entities;
using SignalRHub.Domain.Enums;

namespace SignalRHub.Application;

/// <summary>Who should receive a notification.</summary>
public enum NotificationTarget
{
    /// <summary>Every connected client.</summary>
    All,

    /// <summary>One user (TargetValue is the user id).</summary>
    User,

    /// <summary>Everyone in a room (TargetValue is the room id).</summary>
    Room
}

/// <summary>
/// Input for the "publish a notification" use case. A plain record with no HTTP types,
/// so the same use case can be called from a controller, a worker, or a test.
/// </summary>
/// <param name="Title">Headline.</param>
/// <param name="Message">Body text.</param>
/// <param name="Severity">Importance level.</param>
/// <param name="Target">Who receives it.</param>
/// <param name="TargetValue">User id or room id; ignored when Target is All.</param>
public sealed record PublishNotificationCommand(
    string Title,
    string Message,
    NotificationSeverity Severity,
    NotificationTarget Target,
    string? TargetValue);

/// <summary>
/// The use case: save a notification, then push it live.
/// Order matters: save first, push second. If saving fails nothing is pushed,
/// and users who were offline can still read it from the database later.
/// </summary>
public sealed class PublishNotificationHandler(
    INotificationRepository repository,
    INotificationService realtime,
    ILogger<PublishNotificationHandler> logger)
{
    /// <summary>Runs the use case and returns the DTO that was pushed.</summary>
    /// <exception cref="ArgumentException">Target is User or Room but TargetValue is empty.</exception>
    public async Task<NotificationDto> HandleAsync(
        PublishNotificationCommand command, CancellationToken ct = default)
    {
        string? userId = null;
        string? group = null;

        // Step 1: work out the audience and validate TargetValue.
        switch (command.Target)
        {
            case NotificationTarget.User:
                ArgumentException.ThrowIfNullOrWhiteSpace(command.TargetValue);
                userId = command.TargetValue;
                break;

            case NotificationTarget.Room:
                ArgumentException.ThrowIfNullOrWhiteSpace(command.TargetValue);
                // "room-101" becomes "room:room-101", the same name the hub uses on JoinRoom.
                group = NotificationGroups.Room(command.TargetValue);
                break;
        }

        // Step 2: build the entity. The domain checks title and message.
        var entity = Notification.Create(
            command.Title, command.Message, command.Severity, userId, group);

        // Step 3: save to the database. If this throws, the request fails and nothing is pushed.
        await repository.AddAsync(entity, ct);

        // Step 4: convert to the small shape that browsers receive.
        var dto = entity.ToDto();

        // Step 5: push live through the abstraction. We never touch SignalR types here.
        await (command.Target switch
        {
            NotificationTarget.User => realtime.SendToUserAsync(userId!, dto, ct),
            NotificationTarget.Room => realtime.SendToGroupAsync(group!, dto, ct),
            _ => realtime.BroadcastAsync(dto, ct)
        });

        logger.LogInformation(
            "Published notification {NotificationId} to {Target}", dto.Id, command.Target);

        return dto;
    }
}
