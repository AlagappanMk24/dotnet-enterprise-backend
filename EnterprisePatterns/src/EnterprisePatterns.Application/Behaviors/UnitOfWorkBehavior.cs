using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Application.Common;
using MediatR;

namespace EnterprisePatterns.Application.Behaviors;

/// <summary>
/// Commits the Unit of Work after a command handler succeeds.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Unit of Work + Chain of Responsibility + Decorator (wraps the handler).<br/>
/// <b>Purpose:</b> One commit per command, atomically saving aggregate changes AND outbox messages.<br/>
/// <b>Why It Is Used:</b> Handlers never call SaveChanges, so no use case can forget it or commit twice.
/// If the handler throws, nothing is saved. Queries pass straight through.
/// Note: for payments the external charge happens before this commit; the gateway call is idempotent
/// (order id as idempotency key) so a retry after a failed commit cannot double-charge.
/// </remarks>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommandBase)
        {
            return await next().ConfigureAwait(false);
        }

        var response = await next().ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return response;
    }
}
