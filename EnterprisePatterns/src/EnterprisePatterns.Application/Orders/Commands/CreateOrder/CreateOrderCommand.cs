using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Commands.CreateOrder;

/// <summary>Creates and places a new order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Command (GoF) + CQRS write model, immutable record.<br/>
/// <b>Purpose:</b> Carries everything needed to place an order.<br/>
/// <b>Why It Is Used:</b> A plain object can be validated, logged and (later) queued.
/// </remarks>
public sealed record CreateOrderCommand(
    Guid CustomerId,
    AddressDto ShippingAddress,
    string Currency,
    string? CouponCode,
    IReadOnlyList<CreateOrderItemDto> Items) : ICommand<OrderDto>;