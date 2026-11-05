using EnterprisePatterns.Domain.Orders;

namespace EnterprisePatterns.Application.Abstractions;

/// <summary>Write-side persistence for the Order aggregate.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Repository Pattern + Dependency Inversion Principle.<br/>
/// <b>Purpose:</b> Collection-like access to whole aggregates.<br/>
/// <b>Why It Is Used:</b> Domain and use cases stay testable and database-agnostic.
/// </remarks>
public interface IOrderRepository
{
    void Add(Order order);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}