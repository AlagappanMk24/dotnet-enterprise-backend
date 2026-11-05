using EnterprisePatterns.Domain.ValueObjects;
using System.Collections.Frozen;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>Percentage coupons (WELCOME10 = 10%, FESTIVE15 = 15%).</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Concrete Strategy (GoF) with an immutable lookup table.<br/>
    /// <b>Purpose:</b> Applies a coupon code supplied by the customer.<br/>
    /// <b>Why It Is Used:</b> A FrozenDictionary is read-only after creation, so concurrent reads need no locking.<br/>
    /// In a real system the table would come from a promotions service behind another interface.
    /// </remarks>
    public sealed class CouponDiscountStrategy : IDiscountStrategy
    {
        private static readonly FrozenDictionary<string, decimal> Coupons =
            new Dictionary<string, decimal>
            {
                ["WELCOME10"] = 0.10m,
                ["FESTIVE15"] = 0.15m,
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        public string Name => "Coupon";

        public bool IsApplicable(DiscountContext context) =>
            !string.IsNullOrWhiteSpace(context.CouponCode) && Coupons.ContainsKey(context.CouponCode.Trim());

        public Money Calculate(DiscountContext context) =>
            context.Subtotal.Multiply(Coupons[context.CouponCode!.Trim()]);
    }
}