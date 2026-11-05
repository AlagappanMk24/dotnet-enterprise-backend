

using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>5% off orders whose subtotal reaches 1000 in the order currency.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Concrete Strategy (GoF) + Single Responsibility.<br/>
    /// <b>Purpose:</b> Encourages larger baskets.<br/>
    /// <b>Why It Is Used:</b> Independent of other rules; can be removed by deleting one registration line.
    /// </remarks>
    public sealed class HighValueOrderDiscountStrategy : IDiscountStrategy
    {
        private const decimal Threshold = 1000m;
        private const decimal Rate = 0.05m;

        public string Name => "HighValueOrder5Percent";

        public bool IsApplicable(DiscountContext context) => context.Subtotal.Amount >= Threshold;

        public Money Calculate(DiscountContext context) => context.Subtotal.Multiply(Rate);
    }
}