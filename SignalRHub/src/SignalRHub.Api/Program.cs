using SignalRHub.Api.Extensions;
using SignalRHub.Application;
using SignalRHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Environment);

var app = builder.Build();

app.UseApiPipeline();
app.MapApiEndpoints();

app.Run();