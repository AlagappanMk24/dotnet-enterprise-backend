using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRHub.Application.Abstractions;
using SignalRHub.Infrastructure.BackgroundServices;
using SignalRHub.Infrastructure.Persistence;
using SignalRHub.Infrastructure.Realtime;

namespace SignalRHub.Infrastructure;

/// <summary>Registers everything the Infrastructure layer owns.</summary>
public static class DependencyInjection
{
    /// <summary>Call once from Program.cs.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database. The key "Default" is read from ConnectionStrings in appsettings.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        // Scoped: one repository per request, sharing that request's DbContext.
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Singleton: one tracker for the whole app, because it holds shared state.
        services.AddSingleton<IConnectionTracker, InMemoryConnectionTracker>();

        // Starts automatically when the app starts.
        services.AddHostedService<SystemStatusWorker>();

        return services;
    }
}
