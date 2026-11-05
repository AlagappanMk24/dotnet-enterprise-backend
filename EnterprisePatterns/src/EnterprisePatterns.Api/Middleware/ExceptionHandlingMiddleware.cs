using EnterprisePatterns.Application.Exceptions;
using EnterprisePatterns.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePatterns.Api.Middleware;

/// <summary>
/// Converts any exception into a consistent RFC 9457 ProblemDetails response.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Chain of Responsibility (outermost middleware) + Single Responsibility + Anti-Corruption at the HTTP edge.<br/>
/// <b>Purpose:</b> One central mapping: validation 400, not found 404, state conflict 409, business rule 422, declined payment 402,
/// provider outage 503, unexpected 500.<br/>
/// <b>Why It Is Used:</b> Controllers and handlers contain no try/catch noise; clients always get the same error shape; stack traces
/// and internals are never leaked outside Development. Expected business errors are logged as warnings, only real faults as errors.
/// </remarks>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected: nothing to send. 499 is the conventional "client closed request" code.
            context.Response.StatusCode = 499;
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(context, exception);
        }
    }
    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            AppValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOrderStateTransitionException => (StatusCodes.Status409Conflict, "Invalid state transition"),
            ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            PaymentDeclinedException => (StatusCodes.Status402PaymentRequired, "Payment declined"),
            PaymentGatewayUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Payment provider unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        if (status >= 500 && exception is not PaymentGatewayUnavailableException)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("Request failed with {Status}: {Message}", status, exception.Message);
        }

        ProblemDetails problem = exception is AppValidationException validation
            ? new ValidationProblemDetails(validation.Errors)
            : new ProblemDetails();

        problem.Status = status;
        problem.Title = title;
        problem.Type = $"https://httpstatuses.com/{status}";
        problem.Instance = context.Request.Path;
        problem.Detail = status == StatusCodes.Status500InternalServerError && !environment.IsDevelopment()
            ? "An internal error occurred. Quote the trace id when contacting support."
            : exception.Message;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId))
        {
            problem.Extensions["correlationId"] = correlationId;
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
    }
}