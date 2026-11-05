using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>
/// Skeleton shared by all order event handlers: log, invalidate the read cache, then run the specific reaction.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Template Method (GoF Behavioral) + Observer (each subclass is a subscriber) + DRY.<br/>
/// <b>Purpose:</b> Fixes the algorithm order and lets subclasses supply only <see cref="HandleCoreAsync"/>.<br/>
/// <b>Why It Is Used:</b> Guarantees cache invalidation and logging happen for every order event, even those added later.
/// Handlers run from the outbox processor AFTER the original transaction committed, so a failure here is retried
/// (at-least-once delivery) instead of rolling back the customer's order.
/// </remarks>
public abstract class OrderEventHandlerBase<TEvent>(IOrderReadCache cache, ILogger logger)
    : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IOrderDomainEvent
{
    public async Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        logger.LogInformation("Processing {EventType} {EventId} for order {OrderId}", typeof(TEvent).Name, domainEvent.EventId, domainEvent.OrderId);

        cache.Invalidate(domainEvent.OrderId);
        await HandleCoreAsync(domainEvent, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The varying step of the Template Method. Must be idempotent (events can be delivered twice).</summary>
    protected abstract Task HandleCoreAsync(TEvent domainEvent, CancellationToken cancellationToken);
}