using EnterprisePatterns.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterprisePatterns.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the Order aggregate to relational tables.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Mapping (Data Mapper) + Separation of Concerns + Persistence Ignorance.<br/>
/// <b>Purpose:</b> Keeps every database detail (column names, lengths, indexes, owned types) out of the Domain classes.<br/>
/// <b>Why It Is Used:</b> The Domain stays free of attributes and EF references. Value objects are stored as owned types
/// (columns on the same table), and the private backing field is used for the items collection.
/// </remarks>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        builder.Property(o => o.PaymentReference).HasMaxLength(100);
        builder.Property(o => o.TrackingNumber).HasMaxLength(64);
        builder.Property(o => o.CancellationReason).HasMaxLength(500);
        builder.Property(o => o.CreatedAtUtc).IsRequired();
        builder.Property(o => o.Version).IsConcurrencyToken();

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAtUtc);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
        });
        builder.Navigation(o => o.ShippingAddress).IsRequired();

        builder.OwnsOne(o => o.Discount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(o => o.Discount).IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Computed / infrastructure-only members are not columns.
        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.Subtotal);
        builder.Ignore(o => o.Total);
    }
}
