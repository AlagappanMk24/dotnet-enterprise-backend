using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Domain.Enums;

namespace EnterprisePatterns.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adds cache-aside behaviour to any <see cref="IOrderReadRepository"/>.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Decorator Pattern (GoF Structural, also acting as a Caching Proxy) + Open/Closed Principle.<br/>
/// <b>Purpose:</b> Serves single-order reads from memory and falls back to the wrapped repository on a miss.<br/>
/// <b>Why It Is Used:</b> Caching is added WITHOUT changing the real repository or any handler. Entries expire after a
/// short TTL and are also invalidated by order event handlers, so staleness is bounded (eventual consistency).
/// Lists are not cached because their key space is large and they change constantly.
/// </remarks>
internal sealed class CachedOrderReadRepository(IOrderReadRepository inner, IOrderReadCache cache) : IOrderReadRepository
{
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (cache.TryGet(id, out var cached))
        {
            return cached;
        }

        var order = await inner.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (order is not null)
        {
            cache.Set(order);
        }

        return order;
    }

    public Task<PagedResult<OrderDto>> ListAsync(
        int page, int pageSize, OrderStatus? status, Guid? customerId, CancellationToken cancellationToken) =>
        inner.ListAsync(page, pageSize, status, customerId, cancellationToken);
}