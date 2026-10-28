using Enterprise.OrderApi.Domain.Enums;

namespace Enterprise.OrderApi.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order() { } // EF Core

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "CAD";
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? BackgroundProcessingCompletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(
        Guid customerId,
        string currency,
        IEnumerable<(Guid ProductId, int Quantity, decimal UnitPrice)> items)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a three-letter code.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Currency = currency.ToUpperInvariant(),
            Status = OrderStatus.PendingBackgroundProcessing,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Create(
                order.Id,
                item.ProductId,
                item.Quantity,
                item.UnitPrice));
        }

        if (order._items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.");

        order.TotalAmount = order._items.Sum(item => item.LineTotal);

        return order;
    }

    public void MarkBackgroundProcessingCompleted()
    {
        if (Status == OrderStatus.BackgroundProcessingCompleted)
            return;

        Status = OrderStatus.BackgroundProcessingCompleted;
        BackgroundProcessingCompletedAtUtc = DateTime.UtcNow;
    }
}