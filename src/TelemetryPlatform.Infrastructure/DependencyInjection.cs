using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelemetryPlatform.Application.Equipments;
using TelemetryPlatform.Infrastructure.Persistence;
using TelemetryPlatform.Infrastructure.Persistence.Repositories;

namespace TelemetryPlatform.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Telemetry";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // A connection string é lida ao criar o DbContext (e não no registro),
        // o que permite aos testes de integração trocá-la pela do Testcontainers.
        services.AddDbContext<TelemetryDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' não configurada.");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IEquipmentRepository, EquipmentRepository>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
