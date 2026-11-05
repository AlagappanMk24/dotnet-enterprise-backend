using Microsoft.EntityFrameworkCore;
using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Domain.Enums;
using EnterprisePatterns.Domain.Orders;

namespace EnterprisePatterns.Infrastructure.Persistence.Repositories;

/// <summary>
/// No-tracking read-side repository that returns DTOs.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> CQRS Read Model + Repository + Specification-style filtering (composable IQueryable).<br/>
/// <b>Purpose:</b> Fast paged reads that do not pollute the change tracker.<br/>
/// <b>Why It Is Used:</b> AsNoTracking removes tracking overhead. Paging is applied in the database, never in memory.
/// </remarks>
internal sealed class OrderReadRepository(ApplicationDbContext db) : IOrderReadRepository
{
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return order?.ToDto();
    }

    public async Task<PagedResult<OrderDto>> ListAsync(
        int page, int pageSize, OrderStatus? status, Guid? customerId, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = db.Orders.AsNoTracking();

        if (status is { } statusFilter)
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        if (customerId is { } customerFilter)
        {
            query = query.Where(o => o.CustomerId == customerFilter);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ThenBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<OrderDto>(orders.Select(o => o.ToDto()).ToList(), page, pageSize, total);
    }
}