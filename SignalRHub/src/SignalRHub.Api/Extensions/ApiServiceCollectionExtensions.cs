using System.Text.Json.Serialization;

namespace SignalRHub.Api.Extensions;

/// <summary>The single entry point that registers all API-layer services.</summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>Called once from Program.cs. Registers MVC, error handling, JWT auth, CORS and SignalR.</summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Controllers, with enums accepted and returned as text ("Room" instead of 2).
        services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();   // standard JSON error bodies
        services.AddOpenApi();          // API description document

        return services
            .AddJwtAuthentication()
            .AddClientCors()
            .AddSignalRRealtime(environment);
    }
}
