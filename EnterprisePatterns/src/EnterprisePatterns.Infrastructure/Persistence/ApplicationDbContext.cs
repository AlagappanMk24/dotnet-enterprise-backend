using Microsoft.EntityFrameworkCore;
using EnterprisePatterns.Infrastructure.Persistence.Outbox;
using EnterprisePatterns.Domain.Orders;
using EnterprisePatterns.Domain.Common;

namespace EnterprisePatterns.Infrastructure.Persistence;

/// <summary>
/// EF Core database context: the physical Unit of Work and the home of the transactional outbox write.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Unit of Work (EF's change tracker) + Transactional Outbox (write half) +
/// Optimistic Concurrency + Template Method (overriding SaveChanges).<br/>
/// <b>Purpose:</b> Inside ONE SaveChanges call it (1) bumps order versions, (2) converts raised domain events into
/// outbox rows, and (3) commits everything atomically.<br/>
/// <b>Why It Is Used:</b> Business data and its events commit together or not at all. Lifetime is Scoped
/// (one per request); DbContext is NOT thread safe and must never be shared across concurrent operations.
/// </remarks>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PrepareForSave();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareForSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void PrepareForSave()
    {
        IncrementAggregateVersions();
        MoveDomainEventsToOutbox();
    }

    /// <summary>Optimistic concurrency: a concurrent writer holding the old version will fail its UPDATE.</summary>
    private void IncrementAggregateVersions()
    {
        foreach (var entry in ChangeTracker.Entries<Order>().Where(e => e.State == EntityState.Modified))
        {
            var version = entry.Property(o => o.Version);
            version.CurrentValue = version.OriginalValue + 1;
        }
    }

    /// <summary>Transactional Outbox: events become rows saved in the same transaction as the aggregate.</summary>
    private void MoveDomainEventsToOutbox()
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                OutboxMessages.Add(OutboxMessage.FromDomainEvent(domainEvent));
            }

            aggregate.ClearDomainEvents();
        }
    }
}
