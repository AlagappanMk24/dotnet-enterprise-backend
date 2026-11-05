using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.DTOs;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Queries.ListOrders;

/// <summary>Handles <see cref="ListOrdersQuery"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Repository (read side).<br/>
/// <b>Purpose:</b> Delegates the paged query to the read repository.<br/>
/// <b>Why It Is Used:</b> Keeps SQL concerns inside Infrastructure.
/// </remarks>
public sealed class ListOrdersQueryHandler(IOrderReadRepository readRepository)
    : IRequestHandler<ListOrdersQuery, PagedResult<OrderDto>>
{
    public Task<PagedResult<OrderDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken) =>
        readRepository.ListAsync(request.Page, request.PageSize, request.Status, request.CustomerId, cancellationToken);
}