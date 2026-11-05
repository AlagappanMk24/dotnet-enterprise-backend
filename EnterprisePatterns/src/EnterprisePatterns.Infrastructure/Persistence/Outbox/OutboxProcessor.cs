using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using EnterprisePatterns.Application.Notifications;
using EnterprisePatterns.Domain.Common;

namespace EnterprisePatterns.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background worker that publishes committed outbox messages to in-process subscribers.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Transactional Outbox (relay half) + Observer (publish to handlers) +
/// Polling Publisher + Retry with a poison-message limit.<br/>
/// <b>Purpose:</b> Delivers every domain event at least once, only after its transaction committed.<br/>
/// <b>Why It Is Used:</b> A failing email provider can never roll back an order, and a crash never loses events.
/// Delivery is AT-LEAST-ONCE, so handlers must be idempotent. With several API instances, add row locking
/// (SQL Server: UPDLOCK, READPAST) or run one processor. Registered as a hosted Singleton and creates a fresh
/// DI scope per batch because DbContext is Scoped and must never be captured by a Singleton.
/// </remarks>
internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    // Thread-safe caches: types are resolved once, then reused by every batch.
    private static readonly ConcurrentDictionary<string, Type?> EventTypes = new();
    private static readonly ConcurrentDictionary<Type, Type> NotificationTypes = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.PollIntervalSeconds), timeProvider);

        try
        {
            do
            {
                try
                {
                    await ProcessBatchAsync(settings, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Never let one bad batch kill the worker loop.
                    logger.LogError(ex, "Outbox batch failed; will retry on next tick");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Outbox processor stopping");
        }
    }

    private async Task ProcessBatchAsync(OutboxOptions settings, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < settings.MaxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(settings.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        try
        {
            foreach (var message in messages)
            {
                await DispatchAsync(message, publisher, settings, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            // Persist progress even on shutdown so already-published events are not re-sent.
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task DispatchAsync(OutboxMessage message, IPublisher publisher, OutboxOptions settings, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = Deserialize(message);
            var notificationType = NotificationTypes.GetOrAdd(domainEvent.GetType(), t => typeof(DomainEventNotification<>).MakeGenericType(t));
            var notification = Activator.CreateInstance(notificationType, domainEvent)!;

            await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);

            message.ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
            message.Error = null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message.Length > 1900 ? ex.Message[..1900] : ex.Message;
            logger.LogWarning(ex, "Outbox message {MessageId} failed (attempt {Attempt}/{Max})", message.Id, message.RetryCount, settings.MaxRetries);
        }
    }

    private static IDomainEvent Deserialize(OutboxMessage message)
    {
        var type = EventTypes.GetOrAdd(message.Type, static name =>
        {
            var resolved = Type.GetType(name, throwOnError: false);

            // Security: only trust types from the Domain assembly that really are domain events.
            return resolved is not null
                   && resolved.Assembly == typeof(IDomainEvent).Assembly
                   && typeof(IDomainEvent).IsAssignableFrom(resolved)
                ? resolved
                : null;
        }) ?? throw new InvalidOperationException($"Unknown or untrusted outbox event type '{message.Type}'.");

        return (IDomainEvent)(JsonSerializer.Deserialize(message.Content, type, OutboxJson.Options)
                              ?? throw new InvalidOperationException("Outbox payload deserialized to null."));
    }
}
