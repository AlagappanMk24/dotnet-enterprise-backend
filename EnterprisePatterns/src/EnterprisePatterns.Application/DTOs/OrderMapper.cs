using EnterprisePatterns.Domain.Orders;
using EnterprisePatterns.Domain.ValueObjects;

namespace EnterprisePatterns.Application.DTOs
{/// <summary>
 /// Maps domain objects to DTOs and back.
 /// </summary>
 /// <remarks>
 /// <b>Pattern / Principle Used:</b> Mapper (a simple Adapter between two models) + Single Responsibility.<br/>
 /// <b>Purpose:</b> One explicit, compile-time-checked place for translation.<br/>
 /// <b>Why It Is Used:</b> No reflection or runtime configuration (fast, debuggable, no hidden magic).
 /// </remarks>
    public static class OrderMapper
    {
        public static OrderDto ToDto(this Order order) => new(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Currency,
            order.ShippingAddress.ToDto(),
            order.Items.Select(i => i.ToDto()).ToList(),
            order.Subtotal.Amount,
            order.Discount.Amount,
            order.Total.Amount,
            order.PaymentReference,
            order.TrackingNumber,
            order.CreatedAtUtc,
            order.Version);

        public static OrderItemDto ToDto(this OrderItem item) =>
            new(item.Id, item.Sku, item.ProductName, item.UnitPrice.Amount, item.Quantity, item.LineTotal.Amount);

        public static AddressDto ToDto(this Address address) =>
            new(address.Street, address.City, address.PostalCode, address.Country);

        public static Address ToDomain(this AddressDto dto) =>
            Address.Create(dto.Street, dto.City, dto.PostalCode, dto.Country);
    }
}