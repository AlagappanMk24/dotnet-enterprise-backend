using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.ShipOrder;

/// <summary>Ships a paid order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Command (GoF) + CQRS write model.<br/>
/// <b>Purpose:</b> Records the carrier tracking number.<br/>
/// <b>Why It Is Used:</b> Explicit intent-revealing operation instead of a generic "update order".
/// </remarks>
public sealed record ShipOrderCommand(Guid OrderId, string TrackingNumber) : ICommand<OrderDto>;