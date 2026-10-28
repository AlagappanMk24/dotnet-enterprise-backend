namespace Enterprise.OrderApi.Application.DTOs;

public sealed record PagedOrdersResponse(
    IReadOnlyCollection<OrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);