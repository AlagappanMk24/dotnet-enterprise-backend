namespace EnterprisePatterns.Application.DTOs
{
    /// <summary>Order line returned to clients.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> DTO (immutable record).<br/>
    /// <b>Purpose:</b> Read model of one line.<br/>
    /// <b>Why It Is Used:</b> Stable, serializable shape that is safe to cache and share across threads.
    /// </remarks>
    public sealed record OrderItemDto(Guid Id, string Sku, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
}