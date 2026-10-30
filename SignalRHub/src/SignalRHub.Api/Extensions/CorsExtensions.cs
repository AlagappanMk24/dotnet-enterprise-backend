using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using SignalRHub.Api.Options;

namespace SignalRHub.Api.Extensions;

/// <summary>CORS setup: which websites may call the API and open the hub.</summary>
public static class CorsExtensions
{
    /// <summary>Registers the "SignalRClients" policy from the "Cors" settings.</summary>
    public static IServiceCollection AddClientCors(this IServiceCollection services)
    {
        // Fail at startup if no origin is configured.
        services.AddOptions<ClientCorsSettings>()
            .BindConfiguration(ClientCorsSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors();

        // Build the policy from the validated settings.
        services.AddOptions<CorsOptions>()
            .Configure<IOptions<ClientCorsSettings>>((cors, settings) =>
                cors.AddPolicy(ClientCorsSettings.PolicyName, policy => policy
                    .WithOrigins(settings.Value.AllowedOrigins)   // exact origins only
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));                        // required for SignalR with cookies/auth
        return services;
    }
}
