using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using TelemetryPlatform.Infrastructure;

namespace TelemetryPlatform.IntegrationTests;

/// <summary>
/// Sobe a API inteira em memória apontando para um PostgreSQL real em container (Testcontainers).
/// Exige Docker rodando. O container é criado uma vez por classe de teste (IClassFixture).
/// </summary>
public sealed class TelemetryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(
        [
            new($"ConnectionStrings:{DependencyInjection.ConnectionStringName}", _postgres.GetConnectionString()),
        ]));
    }

    public async ValueTask InitializeAsync() => await _postgres.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
