using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>Reacts to a placed order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Observer (concrete subscriber) + Template Method (concrete step).<br/>
/// <b>Purpose:</b> Sends a confirmation and increments the placed counter.<br/>
/// <b>Why It Is Used:</b> Side effects live outside the Place use case (Single Responsibility).
/// </remarks>
public sealed class OrderPlacedEventHandler(
    IOrderReadCache cache, IOrderMetrics metrics, INotificationSender sender, ILogger<OrderPlacedEventHandler> logger)
    : OrderEventHandlerBase<OrderPlacedDomainEvent>(cache, logger)
{
    protected override Task HandleCoreAsync(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        metrics.RecordPlaced();
        return sender.SendAsync(
            domainEvent.CustomerId,
            "Order confirmed",
            $"Your order {domainEvent.OrderId} for {domainEvent.TotalAmount:0.00} {domainEvent.Currency} has been placed.",
            cancellationToken);
    }
}