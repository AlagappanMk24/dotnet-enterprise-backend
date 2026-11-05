using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Exceptions;
using EnterprisePatterns.Domain.Orders;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Commands.CancelOrder;

/// <summary>Use-case handler for cancellation.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Tell-Don't-Ask + Domain Events (refund flag raised by the aggregate).<br/>
/// <b>Purpose:</b> Cancels the order; the refund reaction happens later through the outbox.<br/>
/// <b>Why It Is Used:</b> The cancel use case stays small and does not call payment services directly.
/// </remarks>
public sealed class CancelOrderCommandHandler(IOrderRepository orders, TimeProvider timeProvider)
    : IRequestHandler<CancelOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Cancel(request.Reason, timeProvider.GetUtcNow());
        return order.ToDto();
    }
}