using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalRHub.Domain.Entities;

namespace SignalRHub.Infrastructure.Persistence.Configurations;

/// <summary>
/// Describes how the Notification entity maps to the Notifications table:
/// column sizes, required fields, and indexes.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.TargetUserId).HasMaxLength(128);
        builder.Property(x => x.TargetGroup).HasMaxLength(128);

        // Store the enum as text ("Info") so the table is readable without a lookup.
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);

        // DomainEvents live only in memory; they are not a column.
        builder.Ignore(x => x.DomainEvents);

        // Speeds up "unread notifications for this user".
        builder.HasIndex(x => new { x.TargetUserId, x.IsRead });
    }
}
