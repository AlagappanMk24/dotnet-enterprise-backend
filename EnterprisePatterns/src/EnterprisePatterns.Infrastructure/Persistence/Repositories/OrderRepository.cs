using Microsoft.EntityFrameworkCore;
using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Domain.Orders;

namespace EnterprisePatterns.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the write-side order repository.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Repository Pattern + Dependency Inversion (implements an Application interface).<br/>
/// <b>Purpose:</b> Loads and tracks whole Order aggregates (with items) and stages new ones.<br/>
/// <b>Why It Is Used:</b> Use cases stay ignorant of EF Core. It does not call SaveChanges: that belongs to the Unit of Work.
/// Scoped lifetime, because it shares the request's DbContext.
/// </remarks>
internal sealed class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public void Add(Order order) => db.Orders.Add(order);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
}