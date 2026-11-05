using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using EnterprisePatterns.Application.Behaviors;
using EnterprisePatterns.Application.Discounts;

namespace EnterprisePatterns.Application;

/// <summary>
/// Registers everything the Application layer needs.
/// </summary>
/// <remarks>
/// <b>Pattern / Principle Used:</b> Composition Root (per layer) + Dependency Injection + Open/Closed Principle.<br/>
/// <b>Purpose:</b> One extension method wires MediatR, the pipeline, validators and discount strategies.<br/>
/// <b>Why It Is Used:</b> The API only calls <c>AddApplication()</c> and knows nothing about internals.
/// Lifetimes: strategies and factory = Singleton (stateless, thread safe); validators = Scoped (FluentValidation default);
/// handlers and behaviors = Transient (MediatR default, stateless).
/// Behavior registration order = execution order: Logging, then Validation, then UnitOfWork, then handler.
/// </remarks>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // Strategy catalogue: adding a rule = adding one line (Open/Closed Principle).
        services.AddSingleton<IDiscountStrategy, BulkQuantityDiscountStrategy>();
        services.AddSingleton<IDiscountStrategy, HighValueOrderDiscountStrategy>();
        services.AddSingleton<IDiscountStrategy, CouponDiscountStrategy>();
        services.AddSingleton<IDiscountStrategyFactory, DiscountStrategyFactory>();

        return services;
    }
}
