using Microsoft.Extensions.DependencyInjection;
using TelemetryPlatform.Application.Equipments;

namespace TelemetryPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<EquipmentService>();

        return services;
    }
}
