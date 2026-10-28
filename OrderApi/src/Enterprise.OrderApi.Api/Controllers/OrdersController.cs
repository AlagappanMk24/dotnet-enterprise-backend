using System.Security.Cryptography;
using System.Text;
using Enterprise.OrderApi.Application.DTOs;
using Enterprise.OrderApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.OrderApi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    IOrderService orderService,
    IConfiguration configuration,
    ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orderService.CreateAsync(
            request, cancellationToken);

        logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId}",
            order.Id,
            order.CustomerId);

        return CreatedAtAction(
            nameof(GetById),
            new { orderId = order.Id },
            order);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(
            orderId, cancellationToken);

        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedOrdersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedOrdersResponse>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await orderService.GetPagedAsync(
            page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpPost("background-processing-complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BackgroundProcessingComplete(
        [FromBody] BackgroundProcessingCompletedRequest request,
        CancellationToken cancellationToken)
    {
        var configuredKey = configuration["InternalApi:ApiKey"];
        var suppliedKey = Request.Headers["X-Internal-Api-Key"].ToString();

        if (string.IsNullOrWhiteSpace(configuredKey) ||
            string.IsNullOrWhiteSpace(suppliedKey) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(configuredKey),
                Encoding.UTF8.GetBytes(suppliedKey)))
        {
            logger.LogWarning(
                "Rejected background-processing callback due to invalid API key.");

            return Unauthorized();
        }

        var updated = await orderService.MarkBackgroundProcessingCompletedAsync(
            request.OrderId,
            cancellationToken);

        if (!updated)
            return NotFound(new { message = "Order was not found." });

        logger.LogInformation(
            "Background processing completed for order {OrderId}",
            request.OrderId);

        return NoContent();
    }
}

public sealed record BackgroundProcessingCompletedRequest(Guid OrderId);