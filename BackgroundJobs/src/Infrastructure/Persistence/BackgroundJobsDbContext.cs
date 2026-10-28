using Enterprise.BackgroundJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.BackgroundJobs.Infrastructure.Persistence
{
    public sealed class BackgroundJobsDbContext(DbContextOptions<BackgroundJobsDbContext> options) : DbContext(options)
    {
        public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BackgroundJobsDbContext).Assembly);
        }
    }
}