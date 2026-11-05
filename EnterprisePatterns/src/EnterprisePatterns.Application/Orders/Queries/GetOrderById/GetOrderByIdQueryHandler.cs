using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.DTOs;
using EnterprisePatterns.Application.Exceptions;
using EnterprisePatterns.Domain.Orders;
using MediatR;

namespace EnterprisePatterns.Application.Orders.Queries.GetOrderById;

/// <summary>Handles <see cref="GetOrderByIdQuery"/>.</summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Mediator + Decorator (the injected read repository is the caching decorator) + DIP.<br/>
/// <b>Purpose:</b> Returns the order read model or throws NotFound.<br/>
/// <b>Why It Is Used:</b> The handler is unaware that caching exists, which is the point of the Decorator.
/// </remarks>
public sealed class GetOrderByIdQueryHandler(IOrderReadRepository readRepository) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) =>
        await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false)
        ?? throw new NotFoundException(nameof(Order), request.Id);
}