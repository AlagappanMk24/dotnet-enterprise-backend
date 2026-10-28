using Enterprise.OrderApi.Application.Interfaces;
using Enterprise.OrderApi.Application.Services;
using Enterprise.OrderApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.OrderApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("OrderDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'OrderDatabase' is missing.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}