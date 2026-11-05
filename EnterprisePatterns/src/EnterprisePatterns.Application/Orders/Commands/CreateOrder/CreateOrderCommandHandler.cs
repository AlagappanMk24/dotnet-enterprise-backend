using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Discounts;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Domain.Orders;
using EnterprisePatterns.Domain.ValueObjects;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Commands.CreateOrder;

/// <summary>Use-case handler: builds, prices and places the order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator (MediatR) + Strategy (pricing via factory) + Factory Method (<c>Order.Create</c>) +
/// Single Responsibility + Dependency Inversion (only abstractions injected).<br/>
/// <b>Purpose:</b> Orchestrates the use case; all business rules stay inside the aggregate.<br/>
/// <b>Why It Is Used:</b> Thin orchestration is easy to unit test. It does not commit: the UnitOfWorkBehavior does, so the order
/// and its OrderPlaced outbox message are saved atomically. Registered Transient by MediatR (stateless).
/// </remarks>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orders,
    IDiscountStrategyFactory discountFactory,
    TimeProvider timeProvider) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var order = Order.Create(request.CustomerId, request.ShippingAddress.ToDomain(), request.Currency, now);

        foreach (var item in request.Items)
        {
            order.AddItem(item.Sku, item.ProductName, Money.Create(item.UnitPrice, order.Currency), item.Quantity);
        }

        var context = new DiscountContext(order.Subtotal, order.Items.Sum(i => i.Quantity), request.CouponCode);
        var strategy = discountFactory.Create(context);
        order.ApplyDiscount(strategy.Calculate(context));

        order.Place(now);
        orders.Add(order);

        return Task.FromResult(order.ToDto());
    }
}