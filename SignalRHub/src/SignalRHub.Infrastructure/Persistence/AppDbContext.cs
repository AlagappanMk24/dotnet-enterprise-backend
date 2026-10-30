using Microsoft.EntityFrameworkCore;
using SignalRHub.Domain.Entities;

namespace SignalRHub.Infrastructure.Persistence;

/// <summary>
/// The EF Core gateway to the database. Each DbSet is a table.
/// Registered as scoped, so one instance lives for one HTTP request.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>The Notifications table.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Finds every IEntityTypeConfiguration in this assembly (for example NotificationConfiguration)
    /// so table rules stay out of the domain entities.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
