namespace EnterprisePatterns.Application.DTOs
{
    /// <summary>Input line when creating an order.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> DTO (immutable record).<br/>
    /// <b>Purpose:</b> Carries raw client input before domain validation.<br/>
    /// <b>Why It Is Used:</b> Clients can never set domain-owned fields such as ids or totals (mass-assignment protection).
    /// </remarks>
    public sealed record CreateOrderItemDto(string Sku, string ProductName, decimal UnitPrice, int Quantity);
}