namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>Chooses the discount strategy for an order.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Factory (GoF Creational) abstraction + Dependency Inversion.<br/>
    /// <b>Purpose:</b> Hides how strategies are selected and combined.<br/>
    /// <b>Why It Is Used:</b> Handlers stay unaware of the concrete rules.
    /// </remarks>
    public interface IDiscountStrategyFactory
    {
        IDiscountStrategy Create(DiscountContext context);
    }
}