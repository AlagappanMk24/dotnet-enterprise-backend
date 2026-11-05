using EnterprisePatterns.Application.Abstractions;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Queries.GetOrderStats;

/// <summary>Handles <see cref="GetOrderStatsQuery"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Singleton consumption (Transient handler, Singleton dependency).<br/>
/// <b>Purpose:</b> Returns an immutable snapshot.<br/>
/// <b>Why It Is Used:</b> A longer-lived dependency injected into a shorter-lived consumer is the safe direction of lifetimes.
/// </remarks>
public sealed class GetOrderStatsQueryHandler(IOrderMetrics metrics) : IRequestHandler<GetOrderStatsQuery, OrderMetricsSnapshot>
{
    public Task<OrderMetricsSnapshot> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(metrics.Snapshot());
}