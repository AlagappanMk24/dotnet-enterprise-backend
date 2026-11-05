using EnterprisePatterns.Domain.Common;
using EnterprisePatterns.Domain.Enums;
using EnterprisePatterns.Domain.Events;
using EnterprisePatterns.Domain.Exceptions;
using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Domain.Orders;

/// <summary>
/// The Order aggregate root: owns its items, discount, payment and shipping data, and every business rule about them.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Aggregate Root (DDD), Factory Method (<see cref="Create"/>), State Machine guards,
/// Domain Events (Observer), Encapsulation, Tell-Don't-Ask.<br/>
/// <b>Purpose:</b> The single place where "what is a legal order?" is decided.<br/>
/// <b>Why It Is Used:</b> Business logic stays in the domain (rich model) rather than in services (anemic model),
/// so rules cannot be bypassed by a careless controller or handler.
/// </remarks>
public sealed class Order : AggregateRoot
{
    private const int MaxLineItems = 50;

    private readonly List<OrderItem> _items = [];

    // Required by EF Core materialisation only.
    private Order()
    {
        Currency = string.Empty;
        ShippingAddress = null!;
        Discount = null!;
    }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public string Currency { get; private set; }

    public Address ShippingAddress { get; private set; }

    public Money Discount { get; private set; }

    public string? PaymentReference { get; private set; }

    public string? TrackingNumber { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>Optimistic concurrency token, incremented by the DbContext on every update.</summary>
    public int Version { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Subtotal => _items.Aggregate(Money.Zero(Currency), (sum, item) => sum.Add(item.LineTotal));

    public Money Total => Subtotal.Subtract(Discount);

    /// <summary>Factory Method: guarantees a new order starts in a valid Draft state.</summary>
    public static Order Create(Guid customerId, Address shippingAddress, string currency, DateTimeOffset now)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer id is required.");
        }

        ArgumentNullException.ThrowIfNull(shippingAddress);
        var normalizedCurrency = Money.NormalizeCurrency(currency);

        return new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            Currency = normalizedCurrency,
            Discount = Money.Zero(normalizedCurrency),
            Status = OrderStatus.Draft,
            CreatedAtUtc = now.UtcDateTime,
        };
    }

    public void AddItem(string sku, string productName, Money unitPrice, int quantity)
    {
        EnsureStatus("add items", OrderStatus.Draft);
        EnsureSameCurrency(unitPrice);

        var existing = _items.FirstOrDefault(i => string.Equals(i.Sku, sku?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            return;
        }

        if (_items.Count >= MaxLineItems)
        {
            throw new DomainException($"An order cannot contain more than {MaxLineItems} distinct items.");
        }

        _items.Add(OrderItem.Create(Id, sku, productName, unitPrice, quantity));
    }

    public void ApplyDiscount(Money discount)
    {
        EnsureStatus("apply a discount", OrderStatus.Draft);
        EnsureSameCurrency(discount);

        if (discount.Amount > Subtotal.Amount)
        {
            throw new DomainException("Discount cannot exceed the order subtotal.");
        }

        Discount = discount;
    }

    public void Place(DateTimeOffset now)
    {
        EnsureStatus("be placed", OrderStatus.Draft);

        if (_items.Count == 0)
        {
            throw new DomainException("An order must contain at least one item before it can be placed.");
        }

        Status = OrderStatus.Placed;
        Touch(now);
        AddDomainEvent(new OrderPlacedDomainEvent(Guid.CreateVersion7(), now, Id, CustomerId, Total.Amount, Currency));
    }

    /// <summary>Lets callers verify payability BEFORE charging a card (avoids charge-then-fail).</summary>
    public void EnsureCanBePaid() => EnsureStatus("be paid", OrderStatus.Placed);

    public void MarkPaid(string paymentReference, DateTimeOffset now)
    {
        EnsureCanBePaid();

        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            throw new DomainException("A payment reference is required.");
        }

        PaymentReference = paymentReference.Trim();
        Status = OrderStatus.Paid;
        Touch(now);
        AddDomainEvent(new OrderPaidDomainEvent(Guid.CreateVersion7(), now, Id, CustomerId, Total.Amount, Currency, PaymentReference));
    }

    public void Ship(string trackingNumber, DateTimeOffset now)
    {
        EnsureStatus("be shipped", OrderStatus.Paid);

        if (string.IsNullOrWhiteSpace(trackingNumber) || trackingNumber.Trim().Length > 64)
        {
            throw new DomainException("A tracking number (max 64 characters) is required.");
        }

        TrackingNumber = trackingNumber.Trim();
        Status = OrderStatus.Shipped;
        Touch(now);
        AddDomainEvent(new OrderShippedDomainEvent(Guid.CreateVersion7(), now, Id, CustomerId, TrackingNumber));
    }

    public void Cancel(string reason, DateTimeOffset now)
    {
        EnsureStatus("be cancelled", OrderStatus.Draft, OrderStatus.Placed, OrderStatus.Paid);

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
        {
            throw new DomainException("A cancellation reason (max 500 characters) is required.");
        }

        var refundRequired = Status == OrderStatus.Paid;
        CancellationReason = reason.Trim();
        Status = OrderStatus.Cancelled;
        Touch(now);
        AddDomainEvent(new OrderCancelledDomainEvent(Guid.CreateVersion7(), now, Id, CustomerId, CancellationReason, refundRequired));
    }

    private void EnsureStatus(string action, params OrderStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOrderStateTransitionException(Id, Status, action);
        }
    }

    private void EnsureSameCurrency(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        if (!string.Equals(money.Currency, Currency, StringComparison.Ordinal))
        {
            throw new DomainException($"Order currency is {Currency} but received {money.Currency}.");
        }
    }

    private void Touch(DateTimeOffset now) => UpdatedAtUtc = now.UtcDateTime;
}
