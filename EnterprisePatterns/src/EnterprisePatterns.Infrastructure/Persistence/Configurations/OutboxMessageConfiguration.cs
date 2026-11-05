using EnterprisePatterns.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterprisePatterns.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the outbox table.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Fluent Mapping + Transactional Outbox support.<br/>
/// <b>Purpose:</b> Defines columns plus an index that makes "next unprocessed messages" cheap.<br/>
/// <b>Why It Is Used:</b> The processor polls constantly; without the index the query degrades as the table grows.
/// </remarks>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Type).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.OccurredOnUtc });
    }
}
