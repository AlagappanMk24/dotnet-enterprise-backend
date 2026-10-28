using Enterprise.OrderApi.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.OrderApi.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or cancelled the request.
            logger.LogDebug("Request was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();

            var statusCode = exception switch
            {
                OrderValidationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == 400
                    ? "Request validation failed"
                    : "An unexpected error occurred.",
                Detail = statusCode == 400
                    ? exception.Message
                    : "Please try again later.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
                problem,
                context.RequestAborted);
        }
    }
}