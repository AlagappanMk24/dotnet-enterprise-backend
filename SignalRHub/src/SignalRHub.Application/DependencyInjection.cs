using Microsoft.Extensions.DependencyInjection;

namespace SignalRHub.Application;

/// <summary>Registers everything the Application layer owns.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Call once from Program.cs. The handler is scoped (one per request)
    /// because it depends on the scoped repository.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PublishNotificationHandler>();
        return services;
    }
}
