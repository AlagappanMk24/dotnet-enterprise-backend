using Enterprise.OrderApi.Domain.Enums;

namespace Enterprise.OrderApi.Application.DTOs;

public sealed record OrderItemResponse(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    DateTime CreatedAtUtc,
    DateTime? BackgroundProcessingCompletedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items);