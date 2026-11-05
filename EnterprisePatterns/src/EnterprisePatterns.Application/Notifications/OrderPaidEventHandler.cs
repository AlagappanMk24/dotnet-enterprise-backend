using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Notifications;

/// <summary>Reacts to a paid order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Observer (concrete subscriber) + Template Method (concrete step).<br/>
/// <b>Purpose:</b> Sends a receipt and increments the paid counter.<br/>
/// <b>Why It Is Used:</b> Receipts can fail and retry independently of the payment itself.
/// </remarks>
public sealed class OrderPaidEventHandler(
    IOrderReadCache cache, IOrderMetrics metrics, INotificationSender sender, ILogger<OrderPaidEventHandler> logger)
    : OrderEventHandlerBase<OrderPaidDomainEvent>(cache, logger)
{
    protected override Task HandleCoreAsync(OrderPaidDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        metrics.RecordPaid();
        return sender.SendAsync(
            domainEvent.CustomerId,
            "Payment received",
            $"We received {domainEvent.AmountPaid:0.00} {domainEvent.Currency} for order {domainEvent.OrderId}. Reference: {domainEvent.PaymentReference}.",
            cancellationToken);
    }
}