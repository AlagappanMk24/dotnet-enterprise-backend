using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using EnterprisePatterns.Api.Middleware;
using EnterprisePatterns.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using EnterprisePatterns.Application;
using EnterprisePatterns.Infrastructure;
using Microsoft.OpenApi;

// =====================================================================================================
// Composition Root (the ONLY place that knows every layer).
// Pattern / Principle Used: Composition Root + Dependency Injection + Dependency Inversion Principle.
// Purpose: Wires abstractions to implementations and builds the middleware pipeline.
// Why It Is Used: All object graphs are assembled in one place, so layers stay decoupled and testable.
// =====================================================================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise Patterns - Orders API",
        Version = "v1",
        Description = "Clean Architecture + CQRS + Transactional Outbox reference implementation (.NET 10).",
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

// Rate limiting protects the API from abusive clients and accidental retry storms.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

// Demo convenience: creates the schema on first run. Production should apply EF Core migrations from a deployment step.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
    await database.EnsureCreatedAsync();
}

// Middleware order matters: correlation id first, then the global exception handler wraps everything below it.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise Patterns - Orders API v1"));
}

app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("fixed");
app.MapHealthChecks("/health");

app.Run();

/// <summary>Exposes the entry point type so integration tests can use WebApplicationFactory.</summary>
public partial class Program { }
