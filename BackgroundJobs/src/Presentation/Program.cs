using Enterprise.BackgroundJobs.Api.Workers;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Enterprise.BackgroundJobs.Infrastructure.External;
using Enterprise.BackgroundJobs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, services, log) => log
    .ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(services)
    .Enrich.FromLogContext().Enrich.WithProperty("Application", "Enterprise.BackgroundJobs")
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<BackgroundJobsDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("BackgroundJobs")
        ?? throw new InvalidOperationException("Missing BackgroundJobs connection string.")));

builder.Services.AddHttpClient<IExternalOrderClient, ExternalOrderClient>(client =>
{
    var baseUrl = builder.Configuration["ExternalOrderApi:BaseUrl"];
    if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)) client.BaseAddress = uri;
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddStandardResilienceHandler();

builder.Services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
builder.Services.AddScoped<IBackgroundJobProcessor, BackgroundJobProcessor>();
builder.Services.AddScoped<IJobEnqueuer, JobEnqueuer>();
builder.Services.Configure<BackgroundJobWorkerOptions>(
    builder.Configuration.GetSection(BackgroundJobWorkerOptions.SectionName));
builder.Services.AddHostedService<BackgroundJobWorker>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.MapControllers();

// Local demo convenience only. Use migrations as a deployment step in production.
if (app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<BackgroundJobsDbContext>()
        .Database.EnsureCreatedAsync();
}
app.Run();