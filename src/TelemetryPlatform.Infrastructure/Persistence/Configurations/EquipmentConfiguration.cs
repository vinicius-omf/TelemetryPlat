using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Infrastructure.Persistence.Configurations;

// Mapeamento via Fluent API: mantém o domínio livre de atributos de persistência.
internal sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Tag).HasMaxLength(Equipment.TagMaxLength).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(Equipment.NameMaxLength).IsRequired();
        builder.Property(e => e.Area).HasMaxLength(Equipment.AreaMaxLength).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Rede de segurança contra duplicidade em requisições concorrentes.
        builder.HasIndex(e => e.Tag).IsUnique();
    }
}
