using Enterprise.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.BackgroundJobs.Infrastructure.Persistence.Configurations
{
    public sealed class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
    {
        public void Configure(EntityTypeBuilder<BackgroundJob> builder)
        {
            builder.ToTable("BackgroundJobs");

            builder.HasKey(job => job.Id);

            builder.Property(job => job.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(job => job.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(job => job.Payload)
                .IsRequired();

            builder.Property(job => job.LastError)
                .HasMaxLength(4000);

            builder.HasIndex(job => new { job.Status, job.NextAttemptAtUtc });
            builder.HasIndex(job => job.LockedUntilUtc);
        }
    }
}