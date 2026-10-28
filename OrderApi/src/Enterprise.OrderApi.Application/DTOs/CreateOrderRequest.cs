namespace Enterprise.OrderApi.Application.DTOs;
public sealed class CreateOrderRequest
{
    public Guid CustomerId { get; init; }

    public string Currency { get; init; } = "CAD";

    public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed class CreateOrderItemRequest
{
    public Guid ProductId { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}