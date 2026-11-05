using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.CancelOrder;

/// <summary>Cancels an order that has not shipped.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Command (GoF) + CQRS write model.<br/>
/// <b>Purpose:</b> Carries the cancellation reason for audit and customer messaging.<br/>
/// <b>Why It Is Used:</b> A required reason gives support teams context.
/// </remarks>
public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand<OrderDto>;