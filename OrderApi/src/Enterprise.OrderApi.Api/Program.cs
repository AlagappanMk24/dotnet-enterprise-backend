using Enterprise.OrderApi.Api.Health;
using Enterprise.OrderApi.Api.Middleware;
using Enterprise.OrderApi.Infrastructure;
using Enterprise.OrderApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOrderInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck(
        "live",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: ["ready"]);

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7041;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Development convenience only. Use EF migrations in production.
if (app.Configuration.GetValue<bool>("Database:ApplyDevelopmentSchema"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "Automatic schema creation is permitted only in Development.");
    }

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<OrderDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();