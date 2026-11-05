using EnterprisePatterns.Domain.Enums;
using EnterprisePatterns.Domain.Events;
using EnterprisePatterns.Domain.Exceptions;
using EnterprisePatterns.Domain.Orders;
using EnterprisePatterns.Domain.ValueObjects;
using Xunit;

namespace Masterclass.UnitTests;

/// <summary>
/// Unit tests for the Order aggregate.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Arrange-Act-Assert + Test Data Builder helpers.<br/>
/// <b>Purpose:</b> Proves business invariants without a database, web server or mocks.<br/>
/// <b>Why It Is Used:</b> A dependency-free Domain gives millisecond-fast, deterministic tests: the payoff of Clean Architecture.
/// </remarks>
public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 31, 10, 0, 0, TimeSpan.Zero);

    private static Address NewAddress() => Address.Create("12 Anna Salai", "Chennai", "600002", "India");

    private static Order NewDraftOrder()
    {
        var order = Order.Create(Guid.NewGuid(), NewAddress(), "inr", Now);
        order.AddItem("KB-1", "Keyboard", Money.Create(1500m, "INR"), 2);
        return order;
    }

    [Fact]
    public void Place_ValidOrder_ChangesStatusAndRaisesEvent()
    {
        var order = NewDraftOrder();

        order.Place(Now);

        Assert.Equal(OrderStatus.Placed, order.Status);
        var raised = Assert.Single(order.DomainEvents);
        var placed = Assert.IsType<OrderPlacedDomainEvent>(raised);
        Assert.Equal(3000m, placed.TotalAmount);
        Assert.Equal("INR", placed.Currency);
    }

    [Fact]
    public void Place_WithoutItems_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), NewAddress(), "INR", Now);

        Assert.Throws<DomainException>(() => order.Place(Now));
    }

    [Fact]
    public void AddItem_SameSku_MergesQuantity()
    {
        var order = NewDraftOrder();

        order.AddItem("kb-1", "Keyboard", Money.Create(1500m, "INR"), 3);

        var line = Assert.Single(order.Items);
        Assert.Equal(5, line.Quantity);
    }

    [Fact]
    public void AddItem_DifferentCurrency_Throws()
    {
        var order = NewDraftOrder();

        Assert.Throws<DomainException>(() => order.AddItem("M-1", "Mouse", Money.Create(10m, "USD"), 1));
    }

    [Fact]
    public void ApplyDiscount_GreaterThanSubtotal_Throws()
    {
        var order = NewDraftOrder();

        Assert.Throws<DomainException>(() => order.ApplyDiscount(Money.Create(3000.01m, "INR")));
    }

    [Fact]
    public void Ship_BeforePayment_ThrowsInvalidTransition()
    {
        var order = NewDraftOrder();
        order.Place(Now);

        Assert.Throws<InvalidOrderStateTransitionException>(() => order.Ship("TRK-1", Now));
    }

    [Fact]
    public void Cancel_AfterPayment_FlagsRefund()
    {
        var order = NewDraftOrder();
        order.Place(Now);
        order.MarkPaid("ch_123", Now);
        order.ClearDomainEvents();

        order.Cancel("Customer changed mind", Now);

        var cancelled = Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.True(cancelled.RefundRequired);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_AfterShipping_Throws()
    {
        var order = NewDraftOrder();
        order.Place(Now);
        order.MarkPaid("ch_123", Now);
        order.Ship("TRK-1", Now);

        Assert.Throws<InvalidOrderStateTransitionException>(() => order.Cancel("Too late", Now));
    }

    [Fact]
    public void Money_DifferentCurrencies_CannotBeAdded()
    {
        var inr = Money.Create(10m, "INR");
        var usd = Money.Create(10m, "USD");

        Assert.Throws<DomainException>(() => inr.Add(usd));
    }

    [Fact]
    public void Money_EqualValues_AreEqual()
    {
        Assert.Equal(Money.Create(10m, "inr"), Money.Create(10.00m, "INR"));
    }
}
