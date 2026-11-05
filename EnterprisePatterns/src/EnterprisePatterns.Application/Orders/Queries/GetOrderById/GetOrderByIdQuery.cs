using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;

namespace EnterprisePatterns.Application.Orders.Queries.GetOrderById;

/// <summary>Fetches one order.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Query (CQRS read model).<br/>
/// <b>Purpose:</b> Side-effect free read.<br/>
/// <b>Why It Is Used:</b> Queries are never committed by the UnitOfWorkBehavior and can be cached.
/// </remarks>
public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;