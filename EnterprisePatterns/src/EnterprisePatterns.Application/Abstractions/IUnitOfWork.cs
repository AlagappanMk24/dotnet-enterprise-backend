namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Atomic commit boundary for a use case.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Unit of Work Pattern + Dependency Inversion Principle.<br/>
    /// <b>Purpose:</b> Commits all aggregate changes (and outbox rows) as one transaction.<br/>
    /// <b>Why It Is Used:</b> Guarantees all-or-nothing persistence; Application depends on this abstraction, not on EF Core.
    /// </remarks>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}