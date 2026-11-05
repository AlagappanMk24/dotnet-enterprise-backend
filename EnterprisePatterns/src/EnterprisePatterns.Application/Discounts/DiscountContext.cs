using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>Immutable input to every discount rule.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Parameter Object (immutable record).<br/>
    /// <b>Purpose:</b> Bundles inputs so strategies share one signature.<br/>
    /// <b>Why It Is Used:</b> New inputs (customer tier...) are added without changing the interface.
    /// </remarks>
    public sealed record DiscountContext(Money Subtotal, int TotalQuantity, string? CouponCode);
}