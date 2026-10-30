using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRHub.Application;
using SignalRHub.Application.Abstractions;
using SignalRHub.Application.DTOs;
using SignalRHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SignalRHub.Api.Controllers;

/// <summary>
/// The JSON body of POST /api/notifications. Validation attributes go on the constructor
/// parameters (no "property:" prefix) because this is a record.
/// </summary>
/// <param name="Title">Headline (required, up to 200 characters).</param>
/// <param name="Message">Body text (required, up to 2000 characters).</param>
/// <param name="Severity">Info, Success, Warning or Critical.</param>
/// <param name="Target">All, User or Room.</param>
/// <param name="TargetValue">User id or room id. Required when Target is User or Room.</param>
public sealed record PublishNotificationRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(2000)] string Message,
    NotificationSeverity Severity,
    NotificationTarget Target,
    string? TargetValue);

/// <summary>
/// HTTP entry point for triggering notifications. Kept thin: it turns an HTTP request into a
/// command and calls the Application layer. All business logic lives in PublishNotificationHandler.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]   // every endpoint needs a valid JWT
public sealed class NotificationsController(
    PublishNotificationHandler handler,
    IConnectionTracker tracker) : ControllerBase
{
    /// <summary>
    /// POST /api/notifications. Saves the notification, then pushes it live.
    /// Only users whose JWT has role "Admin" may call it.
    /// </summary>
    /// <returns>
    /// 202 with the notification when saved and pushed; 400 for bad input;
    /// 401/403 for missing token or role; 500 if saving failed.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NotificationDto>> Publish(
        PublishNotificationRequest request, CancellationToken ct)
    {
        try
        {
            // Map HTTP input to a plain command, then run the use case.
            var dto = await handler.HandleAsync(
                new PublishNotificationCommand(
                    request.Title, request.Message, request.Severity,
                    request.Target, request.TargetValue),
                ct);

            return Accepted(dto);
        }
        catch (ArgumentException ex)
        {
            // For example: Target is Room but TargetValue is empty.
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>GET /api/notifications/connections. How many connections are open right now.</summary>
    [HttpGet("connections")]
    public ActionResult<object> Connections() =>
        Ok(new { total = tracker.TotalConnections });
}
