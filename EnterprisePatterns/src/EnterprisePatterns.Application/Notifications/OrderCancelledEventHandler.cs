using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>Reacts to a cancelled order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Observer (concrete subscriber) + Template Method (concrete step).<br/>
/// <b>Purpose:</b> Informs the customer and flags refunds.<br/>
/// <b>Why It Is Used:</b> Refund work becomes an asynchronous, retryable reaction.
/// </remarks>
public sealed class OrderCancelledEventHandler(
    IOrderReadCache cache, IOrderMetrics metrics, INotificationSender sender, ILogger<OrderCancelledEventHandler> logger)
    : OrderEventHandlerBase<OrderCancelledDomainEvent>(cache, logger)
{
    protected override Task HandleCoreAsync(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        metrics.RecordCancelled();
        var refundText = domainEvent.RefundRequired ? " A refund will be issued." : string.Empty;
        return sender.SendAsync(
            domainEvent.CustomerId,
            "Order cancelled",
            $"Order {domainEvent.OrderId} was cancelled ({domainEvent.Reason}).{refundText}",
            cancellationToken);
    }
}