using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Domain.Enums;

namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Read-side persistence returning DTOs directly.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> CQRS read model + Repository + Interface Segregation (separate from write repository).<br/>
    /// <b>Purpose:</b> Optimized, no-tracking queries that bypass aggregate loading.<br/>
    /// <b>Why It Is Used:</b> Faster reads, and a natural seam for the caching Decorator.
    /// </remarks>
    public interface IOrderReadRepository
    {
        Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<PagedResult<OrderDto>> ListAsync(int page, int pageSize, OrderStatus? status, Guid? customerId, CancellationToken cancellationToken);
    }
}