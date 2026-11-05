using Microsoft.Extensions.Primitives;

namespace EnterprisePatterns.Api.Middleware;

/// <summary>
/// Ensures every request has a correlation id that is logged and echoed back.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Chain of Responsibility (ASP.NET Core middleware pipeline) + Decorator of the request.<br/>
/// <b>Purpose:</b> Reads X-Correlation-ID (or falls back to the trace id), adds it to the log scope and the response header.<br/>
/// <b>Why It Is Used:</b> One id lets you follow a single request across logs, services and support tickets.
/// Middleware instances are Singleton and stateless, so they are thread safe. Inbound values are length-limited and
/// character-checked to prevent log injection.
/// </remarks>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Items[HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out StringValues values) && !StringValues.IsNullOrEmpty(values))
        {
            var candidate = values.ToString();
            if (candidate.Length <= 64 && candidate.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            {
                return candidate;
            }
        }

        return context.TraceIdentifier;
    }
}