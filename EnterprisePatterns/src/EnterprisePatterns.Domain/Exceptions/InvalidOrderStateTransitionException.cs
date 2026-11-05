using EnterprisePatterns.Domain.Enums;

namespace EnterprisePatterns.Domain.Exceptions;

/// <summary>
/// Raised when an action is not allowed in the order's current lifecycle state.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> State guard (State Machine) + Domain Exception hierarchy (Liskov Substitution).<br/>
/// <b>Purpose:</b> Specific error for illegal transitions such as shipping an unpaid order.<br/>
/// <b>Why It Is Used:</b> Lets the API return 409 Conflict (state conflict) instead of a generic business error.
/// </remarks>
public sealed class InvalidOrderStateTransitionException(Guid orderId, OrderStatus currentStatus, string attemptedAction)
    : DomainException($"Order {orderId} is '{currentStatus}' and cannot {attemptedAction}.") { }
