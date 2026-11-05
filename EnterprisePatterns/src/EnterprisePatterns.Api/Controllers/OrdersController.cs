using Microsoft.AspNetCore.Mvc;
using EnterprisePatterns.Api.Contracts;
using EnterprisePatterns.Application.DTOs;
using MediatR;
using EnterprisePatterns.Application.Orders.Commands.CreateOrder;
using EnterprisePatterns.Application.Common;
using EnterprisePatterns.Application.Orders.Queries.ListOrders;
using EnterprisePatterns.Application.Orders.Commands.ShipOrder;
using EnterprisePatterns.Application.Orders.Commands.CancelOrder;
using EnterprisePatterns.Application.Orders.Commands.PayOrder;
using EnterprisePatterns.Domain.Enums;
using EnterprisePatterns.Application.Orders.Queries.GetOrderById;
using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Orders.Queries.GetOrderStats;

namespace EnterprisePatterns.Api.Controllers;

/// <summary>
/// Order management endpoints.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Thin Controller + Mediator (ISender) + Facade over the whole Application layer + Single Responsibility.<br/>
/// <b>Purpose:</b> Translates HTTP to commands/queries and results to status codes. It contains no business logic.<br/>
/// <b>Why It Is Used:</b> Controllers depend on ONE abstraction (ISender), so they never change when use cases change.
/// Errors are not caught here: the global exception middleware maps them. ConfigureAwait(false) is intentionally omitted in this
/// file because ASP.NET Core has no SynchronizationContext; library layers (Application/Infrastructure) do use it.
/// </remarks>
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    /// <summary>Creates and places an order (discounts are applied automatically).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Gets one order (served from cache when possible).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetOrderByIdQuery(id), cancellationToken));

    /// <summary>Lists orders, newest first, with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new ListOrdersQuery(page, pageSize, status, customerId), cancellationToken));

    /// <summary>Pays for an order. Test tokens: tok_ok, tok_decline, tok_flaky, tok_down.</summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OrderDto>> Pay(Guid id, [FromBody] PayOrderRequest request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new PayOrderCommand(id, request.PaymentToken), cancellationToken));

    /// <summary>Ships a paid order.</summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Ship(Guid id, [FromBody] ShipOrderRequest request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ShipOrderCommand(id, request.TrackingNumber), cancellationToken));

    /// <summary>Cancels an order that has not shipped.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new CancelOrderCommand(id, request.Reason), cancellationToken));

    /// <summary>Business counters from the Singleton metrics collector.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(OrderMetricsSnapshot), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderMetricsSnapshot>> Stats(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetOrderStatsQuery(), cancellationToken));
}
