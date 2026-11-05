using EnterprisePatterns.Domain.Common;

namespace EnterprisePatterns.Domain.Events;

/// <summary>
/// Marker for events that belong to a specific order.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Interface Segregation + Domain Event hierarchy.<br/>
/// <b>Purpose:</b> Lets generic handlers (cache invalidation) work with every order event.<br/>
/// <b>Why It Is Used:</b> One template handler can serve all order events without reflection.
/// </remarks>
public interface IOrderDomainEvent : IDomainEvent
{
    Guid OrderId { get; }
}

/// <summary>Raised when an order is placed.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Domain Event as an immutable record (value semantics, thread safe).<br/>
/// <b>Purpose:</b> Announces the fact "order placed" with the data consumers need.<br/>
/// <b>Why It Is Used:</b> Serializable payload for the transactional outbox; consumers never query the aggregate back.
/// </remarks>
public sealed record OrderPlacedDomainEvent(
    Guid EventId, DateTimeOffset OccurredOn, Guid OrderId, Guid CustomerId, decimal TotalAmount, string Currency)
    : IOrderDomainEvent;

/// <summary>Raised when payment is captured.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Domain Event (immutable record).<br/>
/// <b>Purpose:</b> Triggers receipts, fulfilment and metrics.<br/>
/// <b>Why It Is Used:</b> Keeps payment side effects out of the payment use case (Single Responsibility).
/// </remarks>
public sealed record OrderPaidDomainEvent(
    Guid EventId, DateTimeOffset OccurredOn, Guid OrderId, Guid CustomerId, decimal AmountPaid, string Currency, string PaymentReference)
    : IOrderDomainEvent;

/// <summary>Raised when an order leaves the warehouse.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Domain Event (immutable record).<br/>
/// <b>Purpose:</b> Notifies the customer with the tracking number.<br/>
/// <b>Why It Is Used:</b> New reactions can subscribe later without touching the Ship use case (Open/Closed).
/// </remarks>
public sealed record OrderShippedDomainEvent(
    Guid EventId, DateTimeOffset OccurredOn, Guid OrderId, Guid CustomerId, string TrackingNumber)
    : IOrderDomainEvent;

/// <summary>Raised when an order is cancelled.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Domain Event (immutable record).<br/>
/// <b>Purpose:</b> Tells downstream systems whether a refund is required.<br/>
/// <b>Why It Is Used:</b> Refund orchestration becomes an asynchronous, retryable reaction instead of inline code.
/// </remarks>
public sealed record OrderCancelledDomainEvent(
    Guid EventId, DateTimeOffset OccurredOn, Guid OrderId, Guid CustomerId, string Reason, bool RefundRequired)
    : IOrderDomainEvent;
