using Microsoft.EntityFrameworkCore;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Infrastructure.Persistence;

public sealed class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<Equipment> Equipments => Set<Equipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carrega todas as classes IEntityTypeConfiguration<T> deste assembly (pasta Configurations).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelemetryDbContext).Assembly);
    }
}
