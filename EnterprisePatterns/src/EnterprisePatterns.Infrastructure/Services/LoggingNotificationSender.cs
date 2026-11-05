using EnterprisePatterns.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Infrastructure.Services;

/// <summary>
/// Notification channel that writes to the log (placeholder for SMTP / SMS / push providers).
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Strategy (one concrete channel behind INotificationSender) + Dependency Inversion.<br/>
/// <b>Purpose:</b> Makes the whole event flow observable locally with no external service.<br/>
/// <b>Why It Is Used:</b> To go live, register an email-based implementation instead; no handler changes.
/// Stateless, therefore Singleton and thread safe.
/// </remarks>
internal sealed class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task SendAsync(Guid customerId, string subject, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("NOTIFY customer {CustomerId} | {Subject} | {Message}", customerId, subject, message);
        return Task.CompletedTask;
    }
}