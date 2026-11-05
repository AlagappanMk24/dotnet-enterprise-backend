using EnterprisePatterns.Domain.Orders;

namespace EnterprisePatterns.Domain.Enums;

/// <summary>
/// Lifecycle of an order. Allowed transitions are enforced inside <see cref="Order"/>.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> State Machine (guarded transitions): Draft to Placed to Paid to Shipped, with Cancelled as a side exit.<br/>
/// <b>Purpose:</b> Makes the lifecycle explicit and exhaustive.<br/>
/// <b>Why It Is Used:</b> Replaces fragile boolean flags (IsPaid, IsShipped) that can contradict each other.
/// </remarks>
public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Paid = 2,
    Shipped = 3,
    Cancelled = 4,
}
