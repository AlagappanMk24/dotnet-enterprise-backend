using Enterprise.OrderApi.Application.DTOs;
using Enterprise.OrderApi.Application.Exceptions;
using Enterprise.OrderApi.Application.Interfaces;
using Enterprise.OrderApi.Domain.Entities;

namespace Enterprise.OrderApi.Application.Services;

public sealed class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId == Guid.Empty)
            throw new OrderValidationException("Customer ID is required.");

        if (string.IsNullOrWhiteSpace(request.Currency) ||
            request.Currency.Length != 3)
        {
            throw new OrderValidationException(
                "Currency must be a three-letter code.");
        }

        if (request.Items is null || request.Items.Count == 0)
            throw new OrderValidationException("At least one item is required.");

        if (request.Items.Any(x => x.ProductId == Guid.Empty))
            throw new OrderValidationException("Every item needs a product ID.");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new OrderValidationException("Quantity must be greater than zero.");

        if (request.Items.Any(x => x.UnitPrice < 0))
            throw new OrderValidationException("Unit price cannot be negative.");

        var items = request.Items.Select(x =>
            (x.ProductId, x.Quantity, x.UnitPrice));

        Order order;

        try
        {
            order = Order.Create(request.CustomerId, request.Currency, items);
        }
        catch (ArgumentException ex)
        {
            throw new OrderValidationException(ex.Message);
        }

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
            throw new OrderValidationException("Order ID is required.");

        var order = await repository.GetByIdAsync(orderId, cancellationToken);

        return order is null ? null : Map(order);
    }

    public async Task<PagedOrdersResponse> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1)
            throw new OrderValidationException("Page must be at least 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new OrderValidationException(
                "Page size must be between 1 and 100.");

        var totalCount = await repository.CountAsync(cancellationToken);

        var orders = await repository.GetPagedAsync(
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedOrdersResponse(
            orders.Select(Map).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<bool> MarkBackgroundProcessingCompletedAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
            throw new OrderValidationException("Order ID is required.");

        var order = await repository.GetByIdAsync(
            orderId, cancellationToken);

        if (order is null)
            return false;

        // Idempotent: receiving the same callback twice is safe.
        order.MarkBackgroundProcessingCompleted();

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static OrderResponse Map(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Currency,
            order.Status,
            order.CreatedAtUtc,
            order.BackgroundProcessingCompletedAtUtc,
            order.Items.Select(item => new OrderItemResponse(
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToList());
    }
}