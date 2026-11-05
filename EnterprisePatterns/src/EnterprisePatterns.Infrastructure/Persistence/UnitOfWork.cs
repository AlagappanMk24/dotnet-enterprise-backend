using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePatterns.Infrastructure.Persistence;

/// <summary>
/// Application-facing commit boundary backed by the DbContext.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Unit of Work + Adapter (EF exceptions translated to Application exceptions) + DIP.<br/>
/// <b>Purpose:</b> Single SaveChanges call per command; converts EF concurrency errors into a neutral exception.<br/>
/// <b>Why It Is Used:</b> The Application layer never references EF types, and the API maps the neutral exception to HTTP 409.
/// </remarks>
internal sealed class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The order was changed by another request. Reload it and try again.", ex);
        }
    }
}
