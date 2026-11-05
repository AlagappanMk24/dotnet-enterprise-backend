using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Exceptions;
using EnterprisePatterns.Domain.Orders;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Commands.ShipOrder;

/// <summary>Use-case handler for shipping.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Tell-Don't-Ask (the aggregate decides if shipping is legal).<br/>
/// <b>Purpose:</b> Loads the aggregate and delegates to <c>Order.Ship</c>.<br/>
/// <b>Why It Is Used:</b> The state rule lives in one place (the domain), not duplicated in handlers.
/// </remarks>
public sealed class ShipOrderCommandHandler(IOrderRepository orders, TimeProvider timeProvider)
    : IRequestHandler<ShipOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Ship(request.TrackingNumber, timeProvider.GetUtcNow());
        return order.ToDto();
    }
}