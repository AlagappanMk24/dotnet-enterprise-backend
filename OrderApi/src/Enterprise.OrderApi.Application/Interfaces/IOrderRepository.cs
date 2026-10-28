using Enterprise.OrderApi.Domain.Entities;

namespace Enterprise.OrderApi.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}