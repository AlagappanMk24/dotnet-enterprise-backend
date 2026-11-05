using EnterprisePatterns.Application.Discounts;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Orders.Commands.CreateOrder;
using EnterprisePatterns.Domain.ValueObjects;
using Xunit;

namespace EnterprisePatterns.Tests;

/// <summary>
/// Unit tests for the Strategy / Composite / Factory discount engine and the FluentValidation rules.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Arrange-Act-Assert; Strategy pattern verified through its public contract only.<br/>
/// <b>Purpose:</b> Locks in pricing behaviour (best discount wins, no stacking) and request validation.<br/>
/// <b>Why It Is Used:</b> Pricing bugs cost real money; these tests run in milliseconds and need no infrastructure.
/// </remarks>
public sealed class DiscountAndValidationTests
{
    private static DiscountStrategyFactory NewFactory() => new(
    [
        new BulkQuantityDiscountStrategy(),
        new HighValueOrderDiscountStrategy(),
        new CouponDiscountStrategy(),
    ]);

    [Fact]
    public void Factory_NoRuleApplies_ReturnsNullObjectWithZeroDiscount()
    {
        var context = new DiscountContext(Money.Create(100m, "INR"), 1, null);

        var strategy = NewFactory().Create(context);

        Assert.Same(NoDiscountStrategy.Instance, strategy);

        Assert.Equal(0m, strategy.Calculate(context).Amount);
    }

    [Fact]
    public void Composite_PicksBestDiscountWithoutStacking()
    {
        // Bulk 10% = 200, high value 5% = 100, FESTIVE15 = 300 -> 300 wins.
        var context = new DiscountContext(Money.Create(2000m, "INR"), 12, "festive15");

        var strategy = NewFactory().Create(context);

        Assert.Equal(300m, strategy.Calculate(context).Amount);
    }

    [Fact]
    public void Coupon_UnknownCode_IsNotApplicable()
    {
        var context = new DiscountContext(Money.Create(100m, "INR"), 1, "NOPE");

        Assert.False(new CouponDiscountStrategy().IsApplicable(context));
    }

    [Fact]
    public void Validator_ValidCommand_Passes()
    {
        var result = new CreateOrderCommandValidator().Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_EmptyItemsAndBadCurrency_Fails()
    {
        var command = ValidCommand() with { Items = [], Currency = "RUPEES" };
        var result = new CreateOrderCommandValidator().Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.Items));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOrderCommand.Currency));
    }

    private static CreateOrderCommand ValidCommand() => new(
        Guid.NewGuid(),
        new AddressDto("12 Anna Salai", "Chennai", "600002", "India"),
        "INR",
        null,
        [new CreateOrderItemDto("KB-1", "Keyboard", 1500m, 2)]);
}