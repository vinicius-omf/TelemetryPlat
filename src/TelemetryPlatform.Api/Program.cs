using Scalar.AspNetCore;
using TelemetryPlatform.Api.Endpoints;
using TelemetryPlatform.Api.ErrorHandling;
using TelemetryPlatform.Application;
using TelemetryPlatform.Infrastructure;
using TelemetryPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<TelemetryDbContext>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI da API em /scalar
    await app.Services.ApplyMigrationsAsync();
}

app.MapHealthChecks("/health");
app.MapEquipmentEndpoints();

app.Run();
