using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Common;

namespace EnterprisePatterns.Application.Orders.Queries.GetOrderStats;

/// <summary>Reads in-process business counters.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Query (CQRS).<br/>
/// <b>Purpose:</b> Exposes the Singleton metrics object through the same pipeline as every other use case.<br/>
/// <b>Why It Is Used:</b> Consistent logging and error handling even for trivial reads.
/// </remarks>
public sealed record GetOrderStatsQuery : IQuery<OrderMetricsSnapshot> { }