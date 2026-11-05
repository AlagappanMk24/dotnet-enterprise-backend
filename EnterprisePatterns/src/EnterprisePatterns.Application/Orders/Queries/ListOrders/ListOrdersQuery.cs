using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Domain.Enums;

namespace EnterprisePatterns.Application.Orders.Queries.ListOrders;

/// <summary>Lists orders with optional filters.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Query (CQRS) with paging.<br/>
/// <b>Purpose:</b> Bounded, filterable list.<br/>
/// <b>Why It Is Used:</b> Page size is capped, so a client can never request the whole table.
/// </remarks>
public sealed record ListOrdersQuery(int Page = 1, int PageSize = 20, OrderStatus? Status = null, Guid? CustomerId = null)
    : IQuery<PagedResult<OrderDto>>;