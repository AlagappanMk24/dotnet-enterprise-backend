using Enterprise.OrderApi.Application.DTOs;

namespace Enterprise.OrderApi.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<PagedOrdersResponse> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<bool> MarkBackgroundProcessingCompletedAsync(
        Guid orderId,
        CancellationToken cancellationToken);
}