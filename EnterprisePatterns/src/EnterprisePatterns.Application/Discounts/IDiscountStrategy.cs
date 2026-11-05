using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>A single pricing rule.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Strategy Pattern (GoF Behavioral) + Open/Closed Principle.<br/>
    /// <b>Purpose:</b> Encapsulates one interchangeable discount algorithm.<br/>
    /// <b>Why It Is Used:</b> Marketing adds rules by adding classes, never by editing a growing if/else chain.
    /// </remarks>
    public interface IDiscountStrategy
    {
        string Name { get; }

        bool IsApplicable(DiscountContext context);

        Money Calculate(DiscountContext context);
    }
}