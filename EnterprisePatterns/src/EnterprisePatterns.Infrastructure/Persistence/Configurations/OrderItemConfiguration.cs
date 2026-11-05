using EnterprisePatterns.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterprisePatterns.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps order lines.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Mapping (Data Mapper) + Separation of Concerns.<br/>
/// <b>Purpose:</b> Defines the OrderItems table and stores UnitPrice (a Money value object) as owned columns.<br/>
/// <b>Why It Is Used:</b> ValueGeneratedNever is essential: ids are created in the domain, and EF must treat new lines
/// discovered through the aggregate as INSERTs rather than UPDATEs.
/// </remarks>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Sku).HasMaxLength(64).IsRequired();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.HasIndex(i => i.OrderId);

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(i => i.UnitPrice).IsRequired();

        builder.Ignore(i => i.LineTotal);
    }
}