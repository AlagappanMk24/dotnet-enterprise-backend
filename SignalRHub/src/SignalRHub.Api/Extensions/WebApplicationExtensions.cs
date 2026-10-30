using SignalRHub.Api.Constants;
using SignalRHub.Api.Hubs;
using SignalRHub.Api.Options;

namespace SignalRHub.Api.Extensions;

/// <summary>Middleware order and endpoint mapping.</summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// The request pipeline. ORDER MATTERS: each request passes through these top to bottom.
    /// CORS must come before authentication so browser preflight requests are answered first.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();                        // turns unhandled exceptions into a 500 problem response
        app.UseHttpsRedirection();
        app.UseStaticFiles();                             // serves wwwroot (for test.html in development)
        app.UseCors(ClientCorsSettings.PolicyName);
        app.UseAuthentication();                          // who are you? (reads the JWT)
        app.UseAuthorization();                           // are you allowed? ([Authorize] rules)

        return app;
    }

    /// <summary>Maps controllers, the SignalR hub, and development-only endpoints.</summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapDevelopmentEndpoints();    // the /dev/token helper; never mapped in production
        }

        app.MapControllers();
        app.MapHub<NotificationHub>(HubRoutes.Notifications);   // browsers connect here

        return app;
    }
}