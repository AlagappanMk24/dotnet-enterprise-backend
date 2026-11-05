using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>Reacts to a shipped order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Observer (concrete subscriber) + Template Method (concrete step).<br/>
/// <b>Purpose:</b> Sends the tracking number and increments the shipped counter.<br/>
/// <b>Why It Is Used:</b> Customer comms are decoupled from warehouse operations.
/// </remarks>
public sealed class OrderShippedEventHandler(
    IOrderReadCache cache, IOrderMetrics metrics, INotificationSender sender, ILogger<OrderShippedEventHandler> logger)
    : OrderEventHandlerBase<OrderShippedDomainEvent>(cache, logger)
{
    protected override Task HandleCoreAsync(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        metrics.RecordShipped();
        return sender.SendAsync(
            domainEvent.CustomerId,
            "Your order has shipped",
            $"Order {domainEvent.OrderId} is on its way. Tracking number: {domainEvent.TrackingNumber}.",
            cancellationToken);
    }
}