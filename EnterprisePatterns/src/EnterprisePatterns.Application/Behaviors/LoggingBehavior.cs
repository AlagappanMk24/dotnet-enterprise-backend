using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterprisePatterns.Application.Behaviors;

/// <summary>
/// Logs every request and warns when it is slow.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Chain of Responsibility (GoF Behavioral) + Decorator (GoF Structural) + Open/Closed Principle.<br/>
/// <b>Purpose:</b> Cross-cutting observability for every command and query, with timing.<br/>
/// <b>Why It Is Used:</b> Handlers stay free of logging code (Single Responsibility), and slow use cases are visible in production.
/// Registered as an open generic, so one class serves all requests. Stateless, therefore thread safe and Transient.
/// </remarks>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const double SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            return await next().ConfigureAwait(false);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            if (elapsed > SlowRequestThresholdMs)
            {
                logger.LogWarning("Slow request {RequestName} took {ElapsedMs:0} ms", requestName, elapsed);
            }
            else
            {
                logger.LogInformation("Handled {RequestName} in {ElapsedMs:0} ms", requestName, elapsed);
            }
        }
    }
}
