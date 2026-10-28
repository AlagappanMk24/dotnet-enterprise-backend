namespace Enterprise.OrderApi.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem() { } // EF Core

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    public Order Order { get; private set; } = null!;

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        int quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = unitPrice * quantity
        };
    }
}