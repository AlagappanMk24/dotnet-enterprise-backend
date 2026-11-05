using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Exceptions;
using EnterprisePatterns.Domain.Orders;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Commands.PayOrder;

/// <summary>Use-case handler for payment.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Port/Adapter (IPaymentGateway) + Fail-Fast (verify payable BEFORE charging).<br/>
/// <b>Purpose:</b> Charges the card via the gateway and marks the order paid.<br/>
/// <b>Why It Is Used:</b> The state check before the charge avoids charging for an order that cannot be paid.
/// The gateway uses the order id as an idempotency key, protecting against double charges on retries.
/// </remarks>
public sealed class PayOrderCommandHandler(
    IOrderRepository orders,
    IPaymentGateway paymentGateway,
    TimeProvider timeProvider) : IRequestHandler<PayOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.EnsureCanBePaid();

        var result = await paymentGateway
            .ChargeAsync(new PaymentRequest(order.Id, order.Total.Amount, order.Currency, request.PaymentToken), cancellationToken)
            .ConfigureAwait(false);

        if (!result.Succeeded)
        {
            throw new PaymentDeclinedException(order.Id, result.FailureReason ?? "Declined");
        }

        order.MarkPaid(result.TransactionId!, timeProvider.GetUtcNow());
        return order.ToDto();
    }
}