using Enterprise.OrderApi.Application.Interfaces;
using Enterprise.OrderApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.OrderApi.Infrastructure.Persistence;

public sealed class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(
                x => x.Id == orderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return dbContext.Orders.CountAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}