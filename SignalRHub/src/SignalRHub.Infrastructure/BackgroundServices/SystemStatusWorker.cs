using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignalRHub.Application.Abstractions;
using SignalRHub.Application.Common;
using SignalRHub.Application.DTOs;
using SignalRHub.Domain.Enums;

namespace SignalRHub.Infrastructure.BackgroundServices;

/// <summary>
/// A background job that starts with the app and sends a "system healthy" ping to the
/// "system-status" room every 30 seconds. It shows how a non-HTTP trigger can push
/// live messages through the same INotificationService.
/// Nothing is saved to the database, because a status ping is short-lived.
/// To save notifications from a worker, create a scope each tick and resolve
/// PublishNotificationHandler from it (the handler is scoped, the worker is a singleton).
/// </summary>
public sealed class SystemStatusWorker(
    INotificationService notifications,
    ILogger<SystemStatusWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    /// <summary>Called by the host at startup. Runs until the app shuts down.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            // Waits for the next tick; returns false or throws when the app is stopping.
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // Build the message directly. This one is not saved.
                    var ping = new NotificationDto(
                        Guid.NewGuid(),
                        "System status",
                        "All services healthy at " + DateTimeOffset.UtcNow.ToString("HH:mm:ss") + " UTC",
                        NotificationSeverity.Info,
                        DateTimeOffset.UtcNow);

                    // Only tabs that joined the "system-status" room receive it.
                    await notifications.SendToGroupAsync(
                        NotificationGroups.Room(NotificationGroups.SystemStatusRoomId),
                        ping,
                        stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // One failed tick must not kill the worker; log and wait for the next one.
                    logger.LogError(ex, "System status broadcast failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown, nothing to do.
        }
    }
}
