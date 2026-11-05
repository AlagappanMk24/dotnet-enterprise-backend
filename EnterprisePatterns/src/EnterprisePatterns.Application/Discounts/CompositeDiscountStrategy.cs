using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>Combines several strategies and picks the best one for the customer.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Composite Pattern (GoF Structural) - a group of strategies is itself a strategy.<br/>
    /// <b>Purpose:</b> Non-stacking "best discount wins" policy.<br/>
    /// <b>Why It Is Used:</b> Callers treat one rule and many rules identically, and discounts never stack unexpectedly.
    /// </remarks>
    public sealed class CompositeDiscountStrategy(IReadOnlyCollection<IDiscountStrategy> children) : IDiscountStrategy
    {
        public string Name => $"Composite[{string.Join(",", children.Select(c => c.Name))}]";

        public bool IsApplicable(DiscountContext context) => children.Any(c => c.IsApplicable(context));

        public Money Calculate(DiscountContext context)
        {
            var best = Money.Zero(context.Subtotal.Currency);
            foreach (var child in children)
            {
                if (!child.IsApplicable(context))
                {
                    continue;
                }

                var candidate = child.Calculate(context);
                if (candidate.Amount > best.Amount)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
