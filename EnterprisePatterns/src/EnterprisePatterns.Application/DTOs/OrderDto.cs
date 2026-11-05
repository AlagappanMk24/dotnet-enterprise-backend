namespace EnterprisePatterns.Application.DTOs
{
    /// <summary>Order returned to clients.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> DTO / Read Model (CQRS query side).<br/>
    /// <b>Purpose:</b> Flat, serializable projection of the Order aggregate.<br/>
    /// <b>Why It Is Used:</b> Never leaks the aggregate (and its mutators) outside the Application layer.
    /// </remarks>
    public sealed record OrderDto(
        Guid Id,
        Guid CustomerId,
        string Status,
        string Currency,
        AddressDto ShippingAddress,
        IReadOnlyList<OrderItemDto> Items,
        decimal Subtotal,
        decimal Discount,
        decimal Total,
        string? PaymentReference,
        string? TrackingNumber,
        DateTime CreatedAtUtc,
        int Version);
}