using Enterprise.OrderApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.OrderApi.Infrastructure.Persistence;

public sealed class OrderDbContext(
    DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.LineTotal)
                .HasPrecision(18, 2);

            entity.HasIndex(x => x.ProductId);
        });
    }
}