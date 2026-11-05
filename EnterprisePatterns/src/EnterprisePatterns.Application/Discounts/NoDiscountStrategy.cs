using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.Discounts
{
    /// <summary>Zero discount.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Null Object Pattern + Singleton (GoF Creational, thread safe via static initialisation).<br/>
    /// <b>Purpose:</b> Represents "no discount applies" as a real strategy.<br/>
    /// <b>Why It Is Used:</b> Removes null checks from callers. The CLR guarantees static readonly initialisation is thread safe.
    /// </remarks>
    public sealed class NoDiscountStrategy : IDiscountStrategy
    {
        public static readonly NoDiscountStrategy Instance = new();

        private NoDiscountStrategy() { }

        public string Name => "None";

        public bool IsApplicable(DiscountContext context) => true;

        public Money Calculate(DiscountContext context) => Money.Zero(context.Subtotal.Currency);
    }
}