namespace EnterprisePatterns.Application.Discounts;

/// <summary>
/// Builds the right strategy for a given order context.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Factory (GoF Creational) + Dependency Injection of the strategy catalogue + Open/Closed Principle.<br/>
/// <b>Purpose:</b> Filters registered strategies to those that apply and wraps them in a Composite, or returns the Null Object.<br/>
/// <b>Why It Is Used:</b> New strategies self-register in DI and are picked up automatically. The factory holds only
/// immutable references to stateless singletons, so it is a thread-safe Singleton itself.
/// </remarks>
public sealed class DiscountStrategyFactory(IEnumerable<IDiscountStrategy> strategies) : IDiscountStrategyFactory
{
    private readonly IReadOnlyList<IDiscountStrategy> _strategies = strategies.ToArray();

    public IDiscountStrategy Create(DiscountContext context)
    {
        var applicable = _strategies.Where(s => s.IsApplicable(context)).ToArray();
        return applicable.Length == 0 ? NoDiscountStrategy.Instance : new CompositeDiscountStrategy(applicable);
    }
}