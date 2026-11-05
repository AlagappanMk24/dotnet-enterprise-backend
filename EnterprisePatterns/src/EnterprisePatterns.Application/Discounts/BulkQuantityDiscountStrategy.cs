using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>10% off when the customer buys 10 or more units.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Concrete Strategy (GoF) + Single Responsibility.<br/>
    /// <b>Purpose:</b> Rewards bulk purchases.<br/>
    /// <b>Why It Is Used:</b> Stateless, so it is registered as a Singleton and is inherently thread safe.
    /// </remarks>
    public sealed class BulkQuantityDiscountStrategy : IDiscountStrategy
    {
        private const int MinimumQuantity = 10;
        private const decimal Rate = 0.10m;

        public string Name => "BulkQuantity10Percent";

        public bool IsApplicable(DiscountContext context) => context.TotalQuantity >= MinimumQuantity;

        public Money Calculate(DiscountContext context) => context.Subtotal.Multiply(Rate);
    }
}