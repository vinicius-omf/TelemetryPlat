using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Application.Equipments;

/// <summary>
/// Porta de saída: a Application define o contrato e a Infrastructure implementa (EF Core).
/// Assim a regra de negócio não depende do banco — e fica fácil de testar com um fake.
/// </summary>
public interface IEquipmentRepository
{
    Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken);
    Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Equipment>> ListAsync(CancellationToken cancellationToken);
    void Add(Equipment equipment);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
