using EnterprisePatterns.Domain.Common;
using EnterprisePatterns.Domain.Exceptions;
using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Domain.Orders;

/// <summary>
/// One line of an order. Only reachable through its aggregate root <see cref="Order"/>.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Entity inside an Aggregate + Factory Method (internal) + Encapsulation.<br/>
/// <b>Purpose:</b> Holds SKU, name, unit price and quantity with validated invariants.<br/>
/// <b>Why It Is Used:</b> Internal visibility forces all changes through the aggregate root, protecting consistency.
/// </remarks>
public sealed class OrderItem : Entity
{
    private const int MaxQuantityPerLine = 1000;

    private OrderItem()
    {
        Sku = ProductName = string.Empty;
        UnitPrice = null!;
    }

    public Guid OrderId { get; private set; }

    public string Sku { get; private set; }

    public string ProductName { get; private set; }

    public Money UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>Derived value (not persisted).</summary>
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    internal static OrderItem Create(Guid orderId, string? sku, string? productName, Money unitPrice, int quantity)
    {
        ArgumentNullException.ThrowIfNull(unitPrice);

        var cleanSku = sku?.Trim();
        var cleanName = productName?.Trim();

        if (string.IsNullOrEmpty(cleanSku) || cleanSku.Length > 64)
        {
            throw new DomainException("SKU is required and cannot exceed 64 characters.");
        }

        if (string.IsNullOrEmpty(cleanName) || cleanName.Length > 200)
        {
            throw new DomainException("Product name is required and cannot exceed 200 characters.");
        }

        if (unitPrice.Amount <= 0)
        {
            throw new DomainException("Unit price must be greater than zero.");
        }

        EnsureQuantity(quantity);

        return new OrderItem
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            Sku = cleanSku,
            ProductName = cleanName,
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }

    internal void IncreaseQuantity(int additional)
    {
        EnsureQuantity(additional);
        EnsureQuantity(Quantity + additional);
        Quantity += additional;
    }

    private static void EnsureQuantity(int quantity)
    {
        if (quantity < 1 || quantity > MaxQuantityPerLine)
        {
            throw new DomainException($"Quantity must be between 1 and {MaxQuantityPerLine}.");
        }
    }
}
