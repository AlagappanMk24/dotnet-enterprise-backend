using EnterprisePatterns.Application.Abstractions;
using EnterprisePatterns.Infrastructure.Payments;
using EnterprisePatterns.Infrastructure.Persistence;
using EnterprisePatterns.Infrastructure.Persistence.Outbox;
using EnterprisePatterns.Infrastructure.Persistence.Repositories;
using EnterprisePatterns.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterprisePatterns.Infrastructure;

/// <summary>
/// Registers every Infrastructure service.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Composition Root (per layer) + Dependency Inversion + Decorator wiring + Options Pattern.<br/>
/// <b>Purpose:</b> Binds Application abstractions to concrete implementations and chooses the DB provider from configuration.<br/>
/// <b>Why It Is Used:</b> Lifetimes are explicit and justified:
/// SINGLETON = stateless or thread-safe shared state (TimeProvider, cache, metrics, resilience pipeline, vendor client, sender);
/// SCOPED = per-request (DbContext, repositories, Unit of Work, because DbContext is not thread safe);
/// TRANSIENT = lightweight stateless adapter (payment adapter).
/// </remarks>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        // Provider switch keeps local development zero-setup (SQLite) while production uses SQL Server.
        services.AddDbContext<ApplicationDbContext>(builder =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                builder.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
            }
            else
            {
                builder.UseSqlite(connectionString);
            }
        });

        // Persistence (Scoped: share the request's DbContext).
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Decorator wiring (Structural): the cached repository wraps the real one.
        services.AddScoped<OrderReadRepository>();
        services.AddScoped<IOrderReadRepository>(sp => new CachedOrderReadRepository(
            sp.GetRequiredService<OrderReadRepository>(),
            sp.GetRequiredService<IOrderReadCache>()));

        // Singletons: thread-safe, process-wide services.
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOrderReadCache, InMemoryOrderReadCache>();
        services.AddSingleton<IOrderMetrics, OrderMetrics>();
        services.AddSingleton<INotificationSender, LoggingNotificationSender>();

        // Payment adapter + resilience (Adapter + Polly).
        services.AddSingleton<ExternalPaymentProviderClient>();
        services.AddSingleton<PaymentResiliencePipeline>();
        services.AddTransient<IPaymentGateway, PaymentGatewayAdapter>();

        // Transactional outbox processor.
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
