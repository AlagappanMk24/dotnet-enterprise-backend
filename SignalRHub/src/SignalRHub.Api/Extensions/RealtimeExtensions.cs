using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Api.Hubs;
using SignalRHub.Api.Realtime;
using SignalRHub.Application.Abstractions;

namespace SignalRHub.Api.Extensions;

/// <summary>SignalR setup and the services that make real-time push work.</summary>
public static class RealtimeExtensions
{
    /// <summary>Registers SignalR, its options, and our adapter classes.</summary>
    public static IServiceCollection AddSignalRRealtime(
        this IServiceCollection services, IWebHostEnvironment environment)
    {
        services
            .AddSignalR(options =>
            {
                options.EnableDetailedErrors = environment.IsDevelopment();   // show full errors only in development
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);         // server pings the client this often
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);     // drop a client silent for this long (2x keep-alive)
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);          // time allowed to finish the first handshake
                options.MaximumReceiveMessageSize = 32 * 1024;                // reject messages over 32 KB
            })
            // Send enums to browsers as text, matching the HTTP API.
            .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Maps the JWT "sub" claim to the SignalR user id (needed for Clients.User(...)).
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();

        // When Application code asks for INotificationService it gets the SignalR implementation.
        services.AddSingleton<INotificationService, SignalRNotificationService>();

        return services;
    }
}
